# VaettirNet.PixelsDice.Net

A library for interacting with Pixels dice using .NET

## Example usage

```csharp
using VaettirNet.PixelsDice.Net;

var mgr = await PixelsManager.CreateAsync();
await foreach (PixelsDie die in mgr.ScanAsync(findAll:true, connectAll:false, cancellationToken: exit.Token))
{
    die.RollStateChanged +=
        (die, state, face) => Console.WriteLine($"Die {die.PixelId} roll state changed to {state} on face {face}");

    if (!die.IsConnected)
    {
        await die.ConnectAsync();
    }

    Console.WriteLine($"Connected to die {die.PixelId} (color:{die.Colorway}, type:{die.Type}, firmware:{die.BuildTimestamp.ToLocalTime()}");
    die.Blink(5, TimeSpan.FromSeconds(1), Color.Aqua, 0xFF, 0, false);
}
```