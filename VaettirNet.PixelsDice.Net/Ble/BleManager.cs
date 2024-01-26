using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VaettirNet.PixelsDice.Net.Interop;

namespace VaettirNet.PixelsDice.Net.Ble;

public class BleManager
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

    internal BleAdapter GetAdapter(int index)
    {
        return new BleAdapter(NativeMethods.GetAdapter((UIntPtr)index), _dispatcher);
    }

    public static Task<BleManager> CreateAsync()
    {
        var dispatcher = new Dispatcher();
        return dispatcher.Execute(() =>
        {
            NativeMethods.SetLogLevel(_logLevel);
            NativeMethods.SetLogCallback(SimpleBleLog);
            var enabled = NativeMethods.IsBluetoothEnabled();
            return new BleManager(enabled, dispatcher);
        });
    }

    private static BleLogLevel _logLevel =
        #if DEBUG
        BleLogLevel.Error;
        #else
        BleLogLevel.None;
        #endif
    
    private static void SimpleBleLog(BleLogLevel level, IntPtr pModule, IntPtr pFile, uint line, IntPtr pFunction, IntPtr pMessage)
    {
        if (level > _logLevel)
        {
            return;
        }

        string file = Marshal.PtrToStringAnsi(pFile);
        string function = Marshal.PtrToStringAnsi(pFunction);
        string message = Marshal.PtrToStringAnsi(pMessage);
        
        // ReSharper disable once ExplicitCallerInfoArgument
        Logger.Instance.Log((PixelsLogLevel)level, message, file, (int)line, function);
    }

    public static void SetLogLevel(BleLogLevel level)
    {
        _logLevel = level;
        NativeMethods.SetLogLevel(_logLevel);
    }
}