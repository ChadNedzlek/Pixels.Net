using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VaettirNet.Btleplug;

namespace VaettirNet.PixelsDice.Net;

/// <summary>
/// Manager for pixels dice.  All dice should be managed by a single instance of this manager
/// class created from <see cref="CreateAsync"/>. Dice can be found and connected using
/// <see cref="StartScan"/>
/// </summary>
public sealed class PixelsManager : IDisposable
{
    private readonly BtleManager _ble;

    private PixelsManager(BtleManager ble)
    {
        _ble = ble;
    }

    public static Task<PixelsManager> CreateAsync()
    {
        BtleManager manager = BtleManager.Create();
        return Task.FromResult(new PixelsManager(manager));
    }

    /// <summary>
    /// Scan for pixels devices. When a die is found, it will be returned as part of the enumerable. The scan will
    /// continue until the cancellationToken is cancelled
    /// </summary>
    /// <param name="findAll">True to find and return all devices, false to only find devices in savedIdentifiers list</param>
    /// <param name="savedIdentifiers">List of devices to return and connect be default (even if findAll is false)</param>
    /// <param name="cancellationToken">CancellationToken to stop scanning</param>
    /// <returns>Enumerable of all devices found.</returns>
    public async IAsyncEnumerable<PixelsDie> ScanAsync(
        bool findAll,
        IEnumerable<string> savedIdentifiers = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!findAll && savedIdentifiers == null)
            throw new ArgumentException($"One of {nameof(findAll)} or {nameof(savedIdentifiers)} must be set");

        HashSet<ulong> idSet = savedIdentifiers?.Select(s => ulong.Parse(s, NumberStyles.AllowHexSpecifier)).ToHashSet();

        await foreach (BtlePeripheral peripheral in _ble.GetPeripherals([PixelsId.PixelsServiceUuid], false, cancellationToken))
        {
            if (!findAll && !idSet.Contains(peripheral.Address))
            {
                peripheral.Dispose();
                continue;
            }
            
            yield return PixelsDie.Create(peripheral);
        }
    }

    public async IAsyncEnumerable<PixelsDie> ReattachAsync(
        IEnumerable<string> savedIdentifiers,
        TimeSpan? timeout = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        timeout ??= TimeSpan.FromSeconds(30);
        List<string> items = savedIdentifiers.ToList();
        int count = 0;
        var src = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        src.CancelAfter(timeout.Value);

        await foreach (PixelsDie die in ScanAsync(false, items, src.Token))
        {
            count++;
            await die.ConnectAsync();
            yield return die;
            if (count >= items.Count)
            {
                src.Cancel();
                yield break;
            }
        }
    }

    public void Dispose()
    {
        _ble.Dispose();
    }
}

public enum PixelsLogLevel
{
    None = 0,
    Error,
    Warn,
    Info,
    Debug,
    Verbose
}