using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using VaettirNet.PixelsDice.Net;
using VaettirNet.PixelsDice.Net.Animations;
using VaettirNet.PixelsDice.Net.Animations.Actions;
using VaettirNet.PixelsDice.Net.Animations.Conditions;
using VaettirNet.PixelsDice.Net.Animations.Definitions;
using AnimationRule = VaettirNet.PixelsDice.Net.Animations.AnimationRule;

namespace CmdLine;

internal static class Program
{
    private static PixelsLogLevel _logLevel;

    public static async Task Main(string[] args)
    {
        CancellationTokenSource exit = new();
        Console.CancelKeyPress += (_, e) =>
        {
            if (!exit.IsCancellationRequested)
                e.Cancel = true;
            exit.Cancel(true);
        };

        _logLevel = PixelsLogLevel.Error;
        List<string> urls = [];
        bool setAnimations = false;
        OptionSet options = new()
        {
            { "verbose|v", "Verbose logging", _ => _logLevel = PixelsLogLevel.Verbose },
            { "url|u=", "URL to forward roll events", urls.Add },
            { "anim-set", "Set animation set (will overwrite existing animations)", _ => setAnimations = true},
        };
        var rem = options.Parse(args);
        List<string> saved = null;
        if (rem.Count > 0)
        {
            saved = rem;
        }

        if (saved != null)
        {
            Console.WriteLine("Command line specified the following device IDs (will not find others) :");
            foreach (var s in saved)
            {
                Console.WriteLine($"  {s}");
            }
        }

        DieSender sender = null;

        Logger.SetLogLevel(_logLevel);
        var mgr = await PixelsManager.CreateAsync();
        List<PixelsDie> found = new();
        if (urls.Count != 0)
        {
            sender = new DieSender();
            foreach (var u in urls)
            {
                Console.WriteLine($"Forwarding rolls to {u}");
                sender.AddUrl(u);
            }
        }

        try
        {

            var animations = BuildAnimationCollection();
            var animationSet = BuildAnimationSet();
            
            IAsyncEnumerable<PixelsDie> search;
            if (saved != null)
            {
                Console.WriteLine($"Reconnecting to {saved.Count} dice");
                search = mgr.ReattachAsync(saved, cancellationToken: exit.Token);
            }
            else
            {
                Console.WriteLine("Searching for dice (can help to pick up and handle them)");
                search = mgr.ScanAsync(true, true, cancellationToken: exit.Token);
            }

            await foreach (PixelsDie die in search.WithCancellation(exit.Token))
            {
                found.Add(die);
                die.RollStateChanged += DieRolled;
                die.RemoteAction += RemoteAction;
                
                if (!die.IsConnected)
                {
                    Console.WriteLine($"TO SAVE: {die.GetPersistentIdentifier()}");
                    Console.WriteLine("Connecting to die...");
                    await die.ConnectAsync();
                }
                
                sender?.AddDie(die);

                Console.WriteLine(
                    $"Connected to die {die.PixelId} (color:{die.Colorway}, type:{die.Type}, firmware:{die.BuildTimestamp.ToLocalTime()}");

                await die.SendInstantAnimations(animations);
                if (setAnimations)
                {
                    // await die.SendAnimationSet(animationSet);
                }

                die.PlayInstantAnimation(2, 1, 0);
            }
            
            Console.WriteLine($"Found {found.Count} dice!");
            await Task.Delay(Timeout.Infinite, exit.Token);
        }
        catch (OperationCanceledException) when (exit.IsCancellationRequested)
        {
            Console.WriteLine("Exiting");
        }
        finally
        {
            if (sender != null)
                await sender.DisposeAsync();
            
            foreach (var d in found)
            {
                Console.WriteLine($"Disconnecting {d.PixelId}");
                await d.DisposeAsync();
            }
        }
    }

    private static void RemoteAction(PixelsDie die, ushort actionId)
    {
        Console.WriteLine($"Die {die.PixelId}: remote action {actionId}");
    }

    private static AnimationSet BuildAnimationSet()
    {
        return new AnimationSet([
            new AnimationRule(
                new FaceCompareCondition(11, ComparisonType.GreaterThanOrEqual),
                [
                    new RemoteDieAction(55),
                    // new PlayAnimationAction(1, FaceIndex.Current, BuildNoiseAnimation()),
                ])
        ]);
    }

    private static InstantAnimationSet BuildAnimationCollection()
    {
        return new InstantAnimationSet([
            BuildSimpleAnimation(Color.Purple),
            BuildSimpleAnimation(Color.Blue),
            BuildNoiseAnimation(),
            BuildFadeAnimation(),
        ]);
    }

    private static SimpleAnimation BuildSimpleAnimation(Color color)
    {
        return new SimpleAnimation(TimeSpan.FromSeconds(2), 1, color, 1, 0);
    }

    private static NoiseAnimation BuildNoiseAnimation()
    {
        return new NoiseAnimation(
            duration: TimeSpan.FromSeconds(5),
            overallGradient: new RgbTrack(
                ImmutableList.Create(
                    new RgbKeyFrame(Color.Red, TimeSpan.FromSeconds(1)),
                    new RgbKeyFrame(Color.Blue, TimeSpan.FromSeconds(1)),
                    new RgbKeyFrame(Color.Yellow, TimeSpan.FromSeconds(1))
                ),
                0xFFFFFFFF
            ),
            individualGradient: new RgbTrack(
                ImmutableList.Create(
                    new RgbKeyFrame(Color.Purple, TimeSpan.FromSeconds(1)),
                    new RgbKeyFrame(Color.White, TimeSpan.FromSeconds(1)),
                    new RgbKeyFrame(Color.Green, TimeSpan.FromSeconds(1))
                ),
                0xFFFFFFFF),
            blinksPerSecond: NoiseAnimation.MaxBlinksPerSecond,
            blinkDuration: TimeSpan.FromMilliseconds(10),
            fade: 0f,
            colorType: NoiseColorOverrideType.None,
            overallColorVariance: 0.8);
    }

    private static GradientAnimation BuildFadeAnimation()
    {
        return new GradientAnimation(TimeSpan.FromSeconds(5),
            FaceMask.All,
            new RgbTrack([
                new RgbKeyFrame(Color.Red, TimeSpan.FromSeconds(1)),
                new RgbKeyFrame(Color.Blue, TimeSpan.FromSeconds(2)),
                new RgbKeyFrame(Color.Blue, TimeSpan.FromSeconds(3)),
                new RgbKeyFrame(Color.Green, TimeSpan.FromSeconds(4)),
            ], FaceMask.All));
    }

    private static void DieRolled(PixelsDie die, RollState state, int value, int face)
    {
        if (_logLevel > PixelsLogLevel.Info || state == RollState.OnFace)
            Console.WriteLine($"Die {die.PixelId}: {state} face {value} (index: {face})");
    }
}