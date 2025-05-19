using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using VaettirNet.PixelsDice.AnimationImport.Formats;
using VaettirNet.PixelsDice.Net;
using VaettirNet.PixelsDice.Net.Animations;
using VaettirNet.PixelsDice.Net.Animations.Actions;
using VaettirNet.PixelsDice.Net.Animations.Conditions;
using VaettirNet.PixelsDice.Net.Animations.Definitions;

namespace VaettirNet.PixelsDice.AnimationImport;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        string file = null;
        List<string> targetDice = [];
        bool listIds = false;
        bool help = false;
        bool all = false;
        OptionSet options = new()
        {
            { "file|f=", "Animation JSON file (default stdin)", v => file = v },
            { "die|d=", "Target dice ID's", targetDice.Add },
            { "all|a", "Target all dice", v => all = (v != null) },
            { "list-dice-ids", "List discovered dice IDs (no import)", v => listIds = (v != null) },
            { "help|h|?", "Show help message", v => help = (v != null) }
        };

        var rem = options.Parse(args);

        if (help)
        {
            WriteUsage(Console.Out, options);
            return 0;
        }

        if (rem.Count != 0)
        {
            Console.Error.WriteLine($"Unrecognized argument: {rem[0]}");
            WriteUsage(Console.Error, options);
            return 1;
        }

        if (!all && targetDice.Count == 0)
        {
            Console.Error.WriteLine("One of '--all' or at least one '--die' arguments required");
            WriteUsage(Console.Error, options);
            return 1;
        }

        if (listIds)
        {
            CancellationTokenSource src = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                if (src.IsCancellationRequested)
                {
                    return;
                }

                Console.Error.WriteLine("Ending die scan (press again force)...");
                e.Cancel = true;
                src.Cancel();
            };
            await ListDiscoveredDiceIds(src.Token);
            return 0;
        }

        await ImportAnimationProfiles(file, targetDice);

        return 0;
    }

    private static async Task ImportAnimationProfiles(string file, IReadOnlyCollection<string> targetDice)
    {
        await using Stream stream = GetReader(file);
        var profile = await JsonSerializer.DeserializeAsync<ImportProfile>(stream,
            new JsonSerializerOptions(new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            }));
        
        ImmutableList<AnimationRule> rules = profile.Definitions.Select(CreateRule).ToImmutableList();
        AnimationSet animSet = new (rules);
        var programmedAnimation = new NormalsAnimation(
            durationMs: 1000,
            angleTrack: RgbTrack.White,
            axisTrack: new RgbTrack([
                new(Color.Black, 0),
                new(Color.Red, 100),
                new(Color.Orange, 200),
                new(Color.Yellow, 300),
                new(Color.Green, 400),
                new(Color.Blue, 500),
                new(Color.Purple, 600),
                new(Color.White, 700),
                new(Color.Black, 1000)
            ], FaceMask.All),
            timeTrack: RgbTrack.White, 
            fade: 0.2,
            axisOffset: -1,
            axisScale: 1,
            axisScrollSpeed: 2,
            angleScrollSpeed: 0,
            mainGradientColorVariance: 0,
            overrideType: NormalsColorOverrideType.None);
        
        PixelsManager manager = await PixelsManager.CreateAsync();
        if (targetDice == null || targetDice.Count == 0)
        {
            Console.WriteLine("Scanning for dice to set animation profile (CTRL-C to stop)...");
            CancellationTokenSource src = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                if (src.IsCancellationRequested)
                {
                    return;
                }

                Console.Error.WriteLine("Ending die scan (press again force)...");
                e.Cancel = true;
                src.Cancel();
            };
            await foreach (PixelsDie die in manager.ScanAsync(true, cancellationToken: src.Token))
            {
                Console.Write($"Connecting and updating die {die.GetPersistentIdentifier()}... ");
                await die.ConnectAsync();
                await die.SendAnimationSet(animSet);
                Console.WriteLine("Done.");
                await die.SendInstantAnimations(new InstantAnimationSet([
                    programmedAnimation
                ]));
                await die.PlayInstantAnimationAsync(0, 1, FaceIndex.Current);
                await die.DisposeAsync();
            }
        }
        else
        {
            await foreach (PixelsDie die in manager.ReattachAsync(targetDice, TimeSpan.FromMinutes(5)))
            {
                Console.Write($"Updating die {die.GetPersistentIdentifier()}... ");
                await die.SendAnimationSet(animSet);
                Console.WriteLine("Done.");
                await die.SendInstantAnimations(new InstantAnimationSet([
                    programmedAnimation
                ]));
                await die.PlayInstantAnimationAsync(0, 1, FaceIndex.Current);
                await die.DisposeAsync();
            }
        }


        static AnimationRule CreateRule(ImportDefinition def)
        {
            return new AnimationRule(CreateCondition(def.Condition), [new PlayAnimationAction(1, (byte)def.Face, CreateAnimation(def.Animation))]);
        }

        static Condition CreateCondition(ImportCondition cond)
        {
            return cond switch
            {
                RolledImportCondition rolled => new RolledCondition(ToMask(rolled.Faces)),
                RollingImportCondition rolling => new RollingCondition((ushort)rolling.RepeatPeriodInMs),
                _ => throw new NotSupportedException(),
            };
        }

        static Animation CreateAnimation(ImportAnimation imported)
        {
            return imported switch
            {
                CycleImportAnimation anim => new CycleAnimation(
                    anim.DurationInMs,
                    ToMask(anim.Faces),
                    CreateRgbTrack(anim.Track),
                    (byte)anim.Count,
                    anim.Fade,
                    anim.Intensity,
                    anim.Cycles),
                // BlinkIdImportAnimation anim => new BlinkIdAnimation(anim.DurationInMs,
                //     (byte)anim.FramesPerBlink,
                //     anim.Brightness),
                NoiseImportAnimation anim => new NoiseAnimation(anim.DurationInMs,
                    CreateRgbTrack(anim.OverallGradientTrack),
                    CreateRgbTrack(anim.IndividualGradientTrack),
                    anim.BlinksPerSecond,
                    anim.BlinksPerSecondVariance,
                    anim.BlinkDurationMs,
                    anim.Fade,
                    anim.ColorOverrideType,
                    anim.OverallColorVariance),
                GradientImportAnimation anim => new GradientAnimation(
                    anim.DurationInMs,
                    ToMask(anim.Faces),
                    CreateRgbTrack(anim.Track)),
                GradientPatternImportAnimation anim => new GradientPatternAnimation(
                    anim.DurationInMs,
                    anim.Tracks.Select(CreateTrack).ToImmutableList(),
                    CreateRgbTrack(anim.ColorTrack),
                    anim.OverrideWithFace),
                KeyFramedImportAnimation anim => new KeyFramedAnimation(
                    anim.DurationInMs,
                    anim.Tracks.Select(CreateRgbTrack).ToImmutableList()),
                NormalsImportAnimation anim => new NormalsAnimation(
                    anim.DurationInMs,
                    CreateRgbTrack(anim.Angle),
                    CreateRgbTrack(anim.Axis),
                    CreateRgbTrack(anim.Time),
                    anim.Fade,
                    anim.AxisOffset,
                    anim.AxisScale,
                    anim.AxisScrollSpeed,
                    anim.AngleScrollSpeed,
                    anim.MainGradientColorVariance,
                    anim.OverrideType),
                RainbowImportAnimation anim => new RainbowAnimation(
                    anim.DurationInMs,
                    ToMask(anim.Faces),
                    (byte)anim.Count,
                    anim.Fade,
                    anim.Intensity,
                    (byte)anim.Cycles),
                SimpleImportAnimation anim => new SimpleAnimation(
                    anim.DurationInMs,
                    ToMask(anim.Faces),
                    ParseColor(anim.Color),
                    (byte)anim.Count,
                    anim.Fade),
                // WormImportAnimation worm => new WormAnimation(
                //     worm.DurationInMs,
                //     ToMask(worm.Faces),
                //     CreateRgbTrack(worm.Track),
                //     (byte)worm.Count,
                //     worm.Fade,
                //     worm.Intensity,
                //     worm.Cycles),
                _ => throw new ArgumentOutOfRangeException(nameof(imported))
            };
        }

        static Track CreateTrack(ImportTrack track)
        {
            if (track == null)
                return new Track([new KeyFrame(1, 0)], FaceMask.All);
            
            return new Track(track.Frames.Select(CreateFrame).ToImmutableList(), ToMask(track.Leds));
        }

        static RgbTrack CreateRgbTrack(ImportRgbTrack track)
        {
            if (track == null)
                return RgbTrack.White;
            
            return new RgbTrack(
                track.Frames.Select(CreateRgbFrame).ToImmutableList(),
                ToMask(track.Leds)
            );
        }
        
        static KeyFrame CreateFrame(ImportFrame f) => new(f.Intensity, (uint)f.OffsetMs);

        static RgbKeyFrame CreateRgbFrame(ImportRgbFrame f) => new(ParseColor(f.Color), (uint)f.OffsetMs);

        static Color ParseColor(string color)
        {
            if (Regex.IsMatch(color, @"^#[a-fA-F0-9]{6}$"))
            {
                return Color.FromArgb(255, Color.FromArgb(int.Parse(color[1..], NumberStyles.AllowHexSpecifier)));
            }
            if (Regex.IsMatch(color, @"^[a-fA-F0-9]{6}$"))
            {
                return Color.FromArgb(255,Color.FromArgb(int.Parse(color[1..], NumberStyles.AllowHexSpecifier)));
            }
            Match rgbMatch = Regex.Match(color, @"^\s*rgb\s*(\s*(\d)+\s*,\s*(\d)+\s*,\s*(\d)+\s*)\s*$");
            if (rgbMatch.Success)
            {
                return Color.FromArgb(255, int.Parse(rgbMatch.Groups[1].Value), int.Parse(rgbMatch.Groups[2].Value), int.Parse(rgbMatch.Groups[3].Value));
            }

            throw new JsonException($"Unsupported color format: '{color}'");
        }
    }

    private static uint ToMask(ImmutableList<int> list, uint @default = FaceMask.All)
    {
        if (list == null || list.Count == 0)
            return @default;
        return list.Aggregate((uint)0, (agg, v) => agg | (uint)(1 << (v - 1)));
    }

    private static Stream GetReader(string file)
    {
        if (string.IsNullOrEmpty(file) || file == "-")
            return Console.OpenStandardInput();
        return File.OpenRead(file);
    }

    private static async Task ListDiscoveredDiceIds(CancellationToken cancellationToken)
    {
        PixelsManager manager = await PixelsManager.CreateAsync();
        List<PixelsDie> connected = [];
        try
        {
            Console.WriteLine("Scanning for dice...");
            await foreach (PixelsDie die in manager.ScanAsync(true, cancellationToken: cancellationToken))
            {
                connected.Add(die);
                await die.ConnectAsync();
                Console.WriteLine($"Connected to die ID '{die.GetPersistentIdentifier()}'");
                die.RollStateChanged += (d, _, _, _) =>
                {
                    Console.WriteLine($"Die ID '{d.GetPersistentIdentifier()}' was handled");
                };
            }
        }
        finally
        {
            foreach (PixelsDie die in connected)
            {
                await die.DisposeAsync();
            }
        }

    }

    private static void WriteUsage(TextWriter writer, OptionSet options)
    {
        writer.WriteLine("Usage: pxd-anim-import --file <json-file> --die <target-id> --die <target-id>");
        writer.WriteLine();
        options.WriteOptionDescriptions(writer);
    }
}