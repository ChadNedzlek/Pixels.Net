using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using VaettirNet.PixelsDice.Net.Interop;

namespace VaettirNet.PixelsDice.Net.Ble;

internal sealed class BleAdapter : IDisposable
{
    private readonly SafeAdapterHandle _adapter;
    private readonly Dispatcher _dispatcher;

    public BleAdapter(SafeAdapterHandle adapter, Dispatcher dispatcher)
    {
        _adapter = adapter;
        _dispatcher = dispatcher;
    }

    private string _identifier;

    public string Identifier
    {
        get
        {
            if (_identifier == null)
            {
                using StringHandle id = NativeMethods.GetIdentifier(_adapter);
                _identifier = id.Value;
            }

            return _identifier;
        }
    }

    public void Dispose()
    {
        StopScanning();
        _adapter.Dispose();
    }

    private Action<BlePeripheral> _onMatch;
    private ScanCallback _foundCallback;
    private GCHandle _scanHandle;

    private Predicate<SafePeripheralHandle> _match;

    public bool IsScanning => _foundCallback != null;

    public void StartScanning(
        Action<BlePeripheral> matched,
        Predicate<SafePeripheralHandle> match = null,
        IEnumerable<string> preconnectPersistent = null)
    {
        if (Interlocked.Exchange(ref _foundCallback, ScanFound) != null)
            throw new InvalidOperationException();
        
        _scanHandle = GCHandle.Alloc(_foundCallback);

        var preconnect = preconnectPersistent?.ToHashSet();

        match ??= _ => false;

        _onMatch = matched;
        _match = preconnect == null ? match : MatchWithList;
        NativeMethods.OnScanFound(_adapter, _foundCallback, IntPtr.Zero).CheckSuccess();
        NativeMethods.StartScan(_adapter).CheckSuccess();
        return;

        bool MatchWithList(SafePeripheralHandle peripheralHandle)
        {
            var id = BlePeripheral.GetPersistentId(peripheralHandle);
            if (preconnect.Contains(id))
            {
                return true;
            }
            return match(peripheralHandle);
        }
    }

    public void StopScanning()
    {
        if (Interlocked.Exchange(ref _foundCallback, null) == null) return;
        
        NativeMethods.StopScan(_adapter).CheckSuccess();
        _scanHandle.Free();
    }

    private void ScanFound(IntPtr adapter, IntPtr peripheral, IntPtr ignored)
    {
        var peri = new SafePeripheralHandle(peripheral);
        try
        {
            if (_match(peri))
            {
                _onMatch(BlePeripheral.Create(peri, _dispatcher));
            }
            else
            {
                peri.Dispose();
            }
        }
        catch
        {
            peri.Dispose();
            throw;
        }
    }
}