using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Mono.Options;
using VaettirNet.PixelsDice.Net;

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
        OptionSet options = new()
        {
            { "verbose|v", "Verbose logging", _ => _logLevel = PixelsLogLevel.Verbose },
            { "url|u=", "URL to forward roll events", urls.Add },
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

            await foreach (PixelsDie die in search)
            {
                found.Add(die);
                die.RollStateChanged += DieRolled;
                if (!die.IsConnected)
                {
                    Console.WriteLine($"TO SAVE: {die.GetPersistentIdentifier()}");
                    Console.WriteLine("Connecting to die...");
                    await die.ConnectAsync();
                }
                
                sender?.AddDie(die);

                Console.WriteLine(
                    $"Connected to die {die.PixelId} (color:{die.Colorway}, type:{die.Type}, firmware:{die.BuildTimestamp.ToLocalTime()}");
                die.Blink(5, TimeSpan.FromSeconds(1), Color.Aqua, 0xFF, 0, false);
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

    private static void DieRolled(PixelsDie die, RollState state, int value, int face)
    {
        if (_logLevel > PixelsLogLevel.Info || state == RollState.OnFace)
            Console.WriteLine($"Die {die.PixelId}: {state} face {value} (index: {face})");
    }
}