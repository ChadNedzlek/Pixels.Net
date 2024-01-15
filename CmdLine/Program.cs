// See https://aka.ms/new-console-template for more information

using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Pixels.Net;

namespace CmdLine;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        CancellationTokenSource exit = new();
        Console.CancelKeyPress += (_, e) =>
        {
            if (!exit.IsCancellationRequested)
                e.Cancel = true;
            exit.Cancel(true);
        };
        string[] saved = null;
        if (args.Length > 0)
        {
            saved = args;
        }

        var mgr = PixelsManager.Create();
        await foreach (PixelsDie die in mgr.ScanAsync(saved == null, false, saved, exit.Token))
        {
            die.RollStateChanged += DieRolled;
            if (!die.IsConnected)
            {
                Console.WriteLine($"TO SAVE: {die.GetPersistentIdentifier()}");
                Console.WriteLine("Connecting to die...");
                await die.ConnectAsync();
            }
            Console.WriteLine($"Connected to die {die.PixelId} (color:{die.Colorway}, type:{die.Type}, firmware:{die.BuildTimestamp.ToLocalTime()}");
            die.Blink(5, TimeSpan.FromSeconds(1), Color.Aqua, 0xFF, 0, false);
        }
    }

    private static void DieRolled(PixelsDie die, RollState state, int face)
    {
        Console.WriteLine($"Die {die.PixelId} roll state changed to {state} on face {face}");
    }
}