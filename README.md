# Pixels.Net

![Build Status](https://github.com/ChadNedzlek/pixels.net/actions/workflows/main-build.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/VaettirNet.PixelsDice.Net.svg)](https://www.nuget.org/packages/VaettirNet.PixelsDice.Net)

A .NET library for connecting to and interacting with Pixels electronic dice.

## Overview

Pixels.Net is a C# wrapper that allows you to connect to, monitor, and control Pixels electronic dice using Bluetooth Low Energy (BLE). The library provides an interface to communicate with Pixels dice, including reading sensor data, monitoring dice state changes, and playing animations.

## Features

- Connect to Pixels dice via Bluetooth Low Energy
- Monitor dice state (rolling, face up, etc.)
- Receive real-time roll results
- Play and control LED animations
- Import animations from Pixels Studio

## Projects

The solution contains several projects:

- **VaettirNet.PixelsDice.Net**: The core library for interacting with Pixels dice
- **VaettirNet.PixelsDice.AnimationImport**: Tools for importing animations from Pixels Studio
- **BasicDiceMonitor**: Simple console application demonstrating basic dice monitoring
- **WpfUIDice**: WPF example application with UI for interacting with dice
- **CmdLine**: Command-line tool for interacting with Pixels dice

## Getting Started

### Installation

Add the VaettirNet.PixelsDice.Net NuGet package to your project:
```bash
dotnet add package VaettirNet.PixelsDice.Net
```

### Basic Usage
```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VaettirNet.PixelsDice.Net;
// Set log level if needed
Logger.SetLogLevel(PixelsLogLevel.Info);
// Create a PixelsManager
var mgr = await PixelsManager.CreateAsync();
// Create cancellation token for scan control
CancellationTokenSource exit = new();
Console.CancelKeyPress += (_, e) => 
{
    if (!exit.IsCancellationRequested) e.Cancel = true;
    exit.Cancel(true);
};
// Scan for dice
Console.WriteLine("Searching for dice (can help to pick up and handle them)"); List found = new(); 
// Start scanning for dice
await foreach (PixelsDie die in mgr.ScanAsync(true, cancellationToken: exit.Token))
{
    found.Add(die);
    
    // Subscribe to roll state changes
    die.RollStateChanged += (die, state, value, face) => {
        if (state == RollState.OnFace)
            Console.WriteLine($"Die {die.PixelId}: {state} face {value} (index: {face})");
    };
    
    // Subscribe to remote actions
    die.RemoteAction += (die, actionId) => {
        Console.WriteLine($"Die {die.PixelId}: remote action {actionId}");
    };
    
    // Connect to the die if not already connected
    if (!die.IsConnected)
    {
        Console.WriteLine($"ID to reconnect later: {die.GetPersistentIdentifier()}");
        Console.WriteLine("Connecting to die...");
        await die.ConnectAsync();
    }
    
    Console.WriteLine(
        $"Connected to die {die.PixelId} (color:{die.Colorway}, type:{die.Type},
    }
}
Console.WriteLine($"Found {found.Count} dice!");

// Wait for cancellation
try
{
    await Task.Delay(Timeout.Infinite, exit.Token);
} catch (OperationCanceledException) when (exit.IsCancellationRequested)
{
    Console.WriteLine("Exiting");
}
finally
{
    // Disconnect from all dice
    foreach (var die in found)
    {
        Console.WriteLine($"Disconnecting {die.PixelId}");
        await die.DisposeAsync();
    }
}
```

#### Reconnecting to Specific Dice

You can reconnect to previously discovered dice using their persistent identifiers:

```csharp
// List of saved die identifiers
List<string> savedDiceIds = ["12AB34CD"]; 
// Reconnect to specific dice
await foreach (PixelsDie die in mgr.ReattachAsync(savedDiceIds, cancellationToken: exit.Token))
{
    // Handle reconnected die
    Console.WriteLine($"Reconnected to die {die.PixelId}");
}
```


#### Creating and Playing Animations
```csharp
// Create simple animations
var animations = new InstantAnimationSet(
    [
        new SimpleAnimation(
            TimeSpan.FromSeconds(2),
            1,
            Color.Purple,
            1,
            0),
        new GradientAnimation(
            TimeSpan.FromSeconds(1),
            FaceMask.All,
            new RgbTrack(
                [
                    new RgbKeyFrame(Color.Red, TimeSpan.Zero),
                    new RgbKeyFrame(Color.Blue, TimeSpan.FromSeconds(0.5)),
                    new RgbKeyFrame(Color.Green, TimeSpan.FromSeconds(1)),
                ],
                FaceMask.All))]);
// Send animations to the die
await die.SendInstantAnimations(animations);
// Play an animation with index 0
await die.PlayInstantAnimationAsync(0, 1, 0);
```

## Examples

Check the example projects for more detailed usage:

- **BasicDiceMonitor**: Simple console application for monitoring dice rolls
- **WpfUIDice**: Interactive WPF application with UI controls for dice

## Dependencies

- [SimpleBLE](https://github.com/OpenBluetoothToolbox/SimpleBLE) - Bluetooth Low Energy library
- [Pixels Firmware](https://github.com/GameWithPixels/DiceFirmware) - Official Pixels dice firmware

## Documentation

For more detailed documentation, see the comments in the source code and the example applications.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [Pixels by Systemic Games](https://gamewithpixels.com/) - The electronic dice that this library interacts with
- The [SimpleBLE](https://github.com/OpenBluetoothToolbox/SimpleBLE) project for providing the Bluetooth connectivity