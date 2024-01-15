using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pixels.Net.Ble;
using Pixels.Net.Interop;

namespace Pixels.Net;

internal class Dispatcher
{
    private readonly object _prepareLock = new();
    private ConcurrentQueue<(Action action, TaskCompletionSource complete)> _queue;
    private AutoResetEvent _readyEvent;

    internal Task Execute(Action action)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
        {
            action();
            return Task.CompletedTask;
        }

        return SendToDispatcher(action);
    }

    private Task SendToDispatcher(Action action)
    {
        lock (_prepareLock)
        {
            if (_queue == null)
            {
                _queue = new ConcurrentQueue<(Action action, TaskCompletionSource complete)>();
                _readyEvent = new AutoResetEvent(false);
                var t = new Thread(ExecuteQueue);
                t.Start();
            }
        }

        TaskCompletionSource c = new TaskCompletionSource();
        _queue.Enqueue((action, c));
        _readyEvent.Set();
        return c.Task;
    }

    private void ExecuteQueue()
    {
        while(true)
        {
            if (_queue.TryDequeue(out (Action action, TaskCompletionSource complete) item))
            {
                try
                {
                    item.action();
                    item.complete.SetResult();
                }
                catch (Exception e)
                {
                    item.complete.SetException(e);
                }
            }
            else
            {
                _readyEvent.WaitOne();
            }
        }
    }
}

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
    
    public void StartScan(Action<PixelsDie> found, bool findAll, IEnumerable<string> savedIdentifiers = null)
    {
        var adapter = GetAdapter();
        adapter.StartScanning(p => found(PixelsDie.Create(p)), findAll ? IsDie : null, savedIdentifiers);
    }

    public async IAsyncEnumerable<PixelsDie> ScanAsync(
        bool findAll,
        bool connectAll,
        IEnumerable<string> savedIdentifiers = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var adapter = GetAdapter();
        Channel<PixelsDie> channel = Channel.CreateUnbounded<PixelsDie>();
        var saved = savedIdentifiers?.ToHashSet();
        adapter.StartScanning(p => channel.Writer.TryWrite(PixelsDie.Create(p)), findAll ? IsDie : null, savedIdentifiers);

        await using CancellationTokenRegistration reg = cancellationToken.Register(() => adapter.StopScanning());

        await foreach (PixelsDie die in channel.Reader.ReadAllAsync(cancellationToken))
        {
            if (connectAll || (saved?.Contains(die.GetPersistentIdentifier()) ?? false))
            {
                await die.ConnectAsync();
            }

            yield return die;
        }
    }

    private bool IsDie(SafePeripheralHandle peripheral)
    {
        return CouldBeDisconnectedDie(peripheral) && ConnectAndCheck(peripheral);

        static bool CouldBeDisconnectedDie(SafePeripheralHandle peri)
        {
            var count = NativeMethods.GetServiceCount(peri);
            bool foundInformationService = false;
            for (nuint i = 0; i < count; i++)
            {
                BleService service = new BleService();
                NativeMethods.GetService(peri, i, ref service).CheckSuccess();
                if (service.Uuid.Value == "0000180a-0000-1000-8000-00805f9b34fb")
                {
                    foundInformationService = true;
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
                }
            }

            return foundInformationService && dataCount == 1 && foundBasicManufacturerData;
        }

        static bool ConnectAndCheck(SafePeripheralHandle peri)
        {
            bool foundPixelsService = false;
            bool foundNotifyCharacteristic = false;
            bool foundWriteCharacteristic = false;
            NativeMethods.ConnectPeripheral(peri).CheckSuccess();
            try
            {
                var count = NativeMethods.GetServiceCount(peri);
                for (nuint i = 0; i < count; i++)
                {
                    BleService service = new BleService();
                    NativeMethods.GetService(peri, i, ref service).CheckSuccess();
                    if (service.Uuid.Value == PixelsId.ServiceId)
                    {
                        foundPixelsService = true;
                        for (nuint c = 0; c < service.CharacteristicCount; c++)
                        {
                            if (service.Characteristics[c].Uuid.Value == PixelsId.NotifyCharacteristicId)
                                foundNotifyCharacteristic = true;
                            if (service.Characteristics[c].Uuid.Value == PixelsId.WriteCharacteristic)
                                foundWriteCharacteristic = true;
                        }
                    }
                }
            }
            finally
            {
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