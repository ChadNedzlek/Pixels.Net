using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using Pixels.Net.Interop;

namespace Pixels.Net.Ble;

internal class BleManager
{
    private readonly Dispatcher _dispatcher;
    
    private BleManager(bool isEnabled, Dispatcher dispatcher)
    {
        IsEnabled = isEnabled;
        _dispatcher = dispatcher;
    }

    public bool IsEnabled { get; }

    public int AdapterCount
    {
        get
        {
            if (!IsEnabled)
                throw new InvalidOperationException();

            return (int) NativeMethods.GetAdapterCount();
        }
    }

    public BleAdapter GetAdapter(int index)
    {
        return new BleAdapter(NativeMethods.GetAdapter((UIntPtr)index), _dispatcher);
    }

    public static BleManager Create()
    {
        var enabled = NativeMethods.IsBluetoothEnabled();
        return new BleManager(enabled, new Dispatcher());
    }
}