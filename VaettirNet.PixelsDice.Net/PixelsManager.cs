﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VaettirNet.PixelsDice.Net.Ble;
using VaettirNet.PixelsDice.Net.Interop;

namespace VaettirNet.PixelsDice.Net;

/// <summary>
/// Manager for pixels dice.  All dice should be managed by a single instance of this manager
/// class created from <see cref="Create"/>. Dice can be found and connected using
/// <see cref="StartScan"/>
/// </summary>
public sealed class PixelsManager : IDisposable
{
    private readonly BleManager _ble;

    private PixelsManager(BleManager ble)
    {
        _ble = ble;
    }

    public static PixelsManager Create()
    {
        return new(BleManager.Create());
    }

    private BleAdapter _adapter;

    public bool IsScanning => _adapter?.IsScanning ?? false;
    
    /// <summary>
    /// Start background scanning. When a die is found, the <see cref="found"/> callback will be invoked,
    /// passing the un-connected die. To use the die, call <see cref="PixelsDie.ConnectAsync"/>.
    /// </summary>
    /// <param name="found">Callback to be executed when a die is found</param>
    /// <param name="findAll">If true, all pixels dice located will be returned. If false, only dice matching <see cref="savedIdentifiers"/> will be returned.</param>
    /// <param name="savedIdentifiers">Optional. If set, dice matching these identifiers will be returned more quickly, and always, even if findAll is false.</param>
    public void StartScan(Action<PixelsDie> found, bool findAll, IEnumerable<string> savedIdentifiers = null)
    {
        var adapter = GetAdapter();
        adapter.StartScanning(p => found(PixelsDie.Create(p)), findAll ? IsDie : null, savedIdentifiers);
    }

    /// <summary>
    /// Scan for pixels devices. When a die is found, it will be returned as part of the enumerable. The scan will
    /// continue until the cancellationToken is cancelled
    /// </summary>
    /// <param name="findAll">True to find and return all devices, false to only find devices in savedIdentifiers list</param>
    /// <param name="connectAll">True to return all devices in the connected state. False to only connect devices in savedIdentifiers</param>
    /// <param name="savedIdentifiers">List of devices to return and connect be default (even if findAll is false)</param>
    /// <param name="cancellationToken">CancellationToken to stop scanning</param>
    /// <returns>Enumerable of all devices found. If <see cref="connectAll"/> is true or <see cref="savedIdentifiers"/>
    /// matches a device, the die will already be connected</returns>
    public async IAsyncEnumerable<PixelsDie> ScanAsync(
        bool findAll,
        bool connectAll,
        IEnumerable<string> savedIdentifiers = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!findAll && savedIdentifiers == null)
            throw new ArgumentException($"One of {nameof(findAll)} or {nameof(savedIdentifiers)} must be set");
        
        var adapter = GetAdapter();
        Channel<PixelsDie> channel = Channel.CreateUnbounded<PixelsDie>();
        var saved = savedIdentifiers?.ToHashSet();
        adapter.StartScanning(p => channel.Writer.TryWrite(PixelsDie.Create(p)), findAll ? IsDie : null, saved);

        CancellationTokenRegistration reg = cancellationToken.Register(() => adapter.StopScanning());
        await using ConfiguredAsyncDisposable _ = reg.ConfigureAwait(false);

        await foreach (PixelsDie die in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            if (connectAll || (saved?.Contains(die.GetPersistentIdentifier()) ?? false))
            {
                await die.ConnectAsync().ConfigureAwait(false);
            }

            yield return die;
        }
    }

    public void StopScan()
    {
        _adapter.StopScanning();
    }

    public IAsyncEnumerable<PixelsDie> ReconnectAsync(IEnumerable<string> savedIdentifiers, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        timeout ??= TimeSpan.FromSeconds(30);
        var items = savedIdentifiers.ToList();

        var channel = Channel.CreateUnbounded<PixelsDie>();
        int count = 0;
        
        CancellationTokenSource src = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        src.CancelAfter(timeout.Value);
        src.Token.Register(() =>
        {
            channel.Writer.TryComplete();
            StopScan();
        });
        
        StartScan(FoundDie, false, items);

        return channel.Reader.ReadAllAsync(cancellationToken);

        void FoundDie(PixelsDie die)
        {
            channel.Writer.TryWrite(die);
            count++;
            if (count == items.Count)
            {
                channel.Writer.TryComplete();
                StopScan();
            }
        }
    }

    private bool IsDie(SafePeripheralHandle peripheral)
    {
        return CouldBeDisconnectedDie(peripheral) ?? ConnectAndCheck(peripheral);

        static bool? CouldBeDisconnectedDie(SafePeripheralHandle peri)
        {
            var count = NativeMethods.GetServiceCount(peri);
            if (Logger.Instance.ShouldLog(PixelsLogLevel.Verbose))
            {
                using StringHandle id = NativeMethods.GetPeripheralIdentifier(peri);
                using StringHandle address = NativeMethods.GetPeripheralAddress(peri);
                Logger.Instance.Log(PixelsLogLevel.Verbose, $"Found device (id: {id.Value}, address: {address.Value})");
            }

            bool foundInformationService = false;
            bool foundPixelsServices = false;
            for (nuint i = 0; i < count; i++)
            {
                BleService service = new BleService();
                NativeMethods.GetService(peri, i, ref service).CheckSuccess();
                if (service.Uuid.Value == PixelsId.InfoServiceId)
                {
                    foundInformationService = true;
                    Logger.Instance.Log(PixelsLogLevel.Info, "Found Info service");
                }
                else if (service.Uuid.Value == PixelsId.PixelsServiceId)
                {
                    foundPixelsServices = true;
                    Logger.Instance.Log(PixelsLogLevel.Info, "Found Pixels service");
                }
                else
                {
                    Logger.Instance.Log(PixelsLogLevel.Verbose, $"Found irrelevant service: {service.Uuid.Value}");
                }
            }

            var dataCount = NativeMethods.GetManufacturerDataCount(peri);
            bool foundBasicManufacturerData = false;
            for (nuint i = 0; i < dataCount; i++)
            {
                BleManufacturerData data = new();
                NativeMethods.GetManufacturerData(peri, i, ref data);
                if (data.DataLength == 5)
                {
                    foundBasicManufacturerData = true;
                    Logger.Instance.Log(PixelsLogLevel.Info, "Found manufacture data with 5 bytes");
                }
                else
                {
                    Logger.Instance.Log(PixelsLogLevel.Verbose, $"Found manufacture data with incorrect length {data.DataLength} bytes");
                }
            }

            if (foundPixelsServices && foundInformationService)
                return true;
            if (foundInformationService && dataCount == 1 && foundBasicManufacturerData)
                return null;
            return false;
        }

        static bool ConnectAndCheck(SafePeripheralHandle peri)
        {
            bool foundPixelsService = false;
            bool foundNotifyCharacteristic = false;
            bool foundWriteCharacteristic = false;
            Logger.Instance.Log(PixelsLogLevel.Verbose, "Connecting to device to scan other services");
            NativeMethods.ConnectPeripheral(peri).CheckSuccess();
            try
            {
                var count = NativeMethods.GetServiceCount(peri);
                for (nuint i = 0; i < count; i++)
                {
                    BleService service = new BleService();
                    NativeMethods.GetService(peri, i, ref service).CheckSuccess();
                    if (service.Uuid.Value == PixelsId.PixelsServiceId)
                    {
                        Logger.Instance.Log(PixelsLogLevel.Info, "Found active Pixels service");
                        foundPixelsService = true;
                        for (nuint c = 0; c < service.CharacteristicCount; c++)
                        {
                            if (service.Characteristics[c].Uuid.Value == PixelsId.NotifyCharacteristicId)
                            {
                                Logger.Instance.Log(PixelsLogLevel.Info, "Found notify characteristic");
                                foundNotifyCharacteristic = true;
                            } else if (service.Characteristics[c].Uuid.Value == PixelsId.WriteCharacteristic)
                            {
                                Logger.Instance.Log(PixelsLogLevel.Info, "Found write characteristic");
                                foundWriteCharacteristic = true;
                            }
                            else
                            {
                                Logger.Instance.Log(PixelsLogLevel.Verbose,
                                    $"Found irrelevant characteristic: {service.Characteristics[c].Uuid.Value}");
                            }
                        }
                    }
                }
            }
            finally
            {
                Logger.Instance.Log(PixelsLogLevel.Verbose, "Disconnecting device");
                NativeMethods.DisconnectPeripheral(peri).CheckSuccess();
            }

            return foundPixelsService && foundNotifyCharacteristic && foundWriteCharacteristic;
        }
    }

    private BleAdapter GetAdapter()
    {
        if (_adapter != null)
            return _adapter;

        return _adapter = _ble.GetAdapter(0);
    }

    public void Dispose()
    {
        _adapter?.Dispose();
    }
}

public enum PixelsLogLevel
{
    None = 0,
    Fatal,
    Error,
    Warn,
    Info,
    Debug,
    Verbose
}