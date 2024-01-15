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
        #if DEBUG
        NativeMethods.SetLogLevel(BleLogLevel.Info);
        #else
        NativeMethods.SetLogLevel(BleLogLevel.None);
        #endif
        NativeMethods.SetLogCallback(SimpleBleLog);
        var enabled = NativeMethods.IsBluetoothEnabled();
        return new BleManager(enabled, new Dispatcher());
    }

    private static void SimpleBleLog(BleLogLevel level, IntPtr pModule, IntPtr pFile, uint line, IntPtr pFunction, IntPtr pMessage)
    {
        if (level > BleLogLevel.Error)
        {
            return;
        }

        string module = Marshal.PtrToStringAnsi(pModule);
        string file = Marshal.PtrToStringAnsi(pFile);
        string function = Marshal.PtrToStringAnsi(pFunction);
        string message = Marshal.PtrToStringAnsi(pMessage);
        
        Console.WriteLine($"[{level}] {module}: {file}:{line} in {function}: {message}");
    }
}