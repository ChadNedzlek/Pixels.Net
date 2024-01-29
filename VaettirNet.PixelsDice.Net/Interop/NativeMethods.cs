using System;
using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Interop;

internal static partial class NativeMethods
{
    public const string SimpleBleLibraryName = "simpleble-c";

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_adapter_is_bluetooth_enabled")]
    [return: MarshalAs(UnmanagedType.U1)]
    internal static partial bool IsBluetoothEnabled();

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_adapter_get_count")]
    internal static partial nuint GetAdapterCount();

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_adapter_get_handle")]
    internal static partial SafeAdapterHandle GetAdapter(nuint index);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_adapter_release_handle")]
    internal static partial void ReleaseAdapter(IntPtr handle);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_release_handle")]
    internal static partial void ReleasePeripheral(IntPtr handle);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_identifier")]
    internal static partial StringHandle GetIdentifier(SafeAdapterHandle handle);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_free")]
    internal static partial void ReleaseHandle(IntPtr handle);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_adapter_scan_start")]
    internal static partial CallResult StartScan(SafeAdapterHandle adapter);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_adapter_scan_stop")]
    internal static partial CallResult StopScan(SafeAdapterHandle adapter);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_adapter_set_callback_on_scan_found")]
    internal static partial CallResult OnScanFound(SafeAdapterHandle adapter, ScanCallback callback, IntPtr data);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_services_count")]
    internal static partial nuint GetServiceCount(SafePeripheralHandle peripheral);

    [DllImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_services_get")]
    internal static extern CallResult GetService(SafePeripheralHandle peripheral, nuint index, ref BleService service);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_manufacturer_data_count")]
    internal static partial nuint GetManufacturerDataCount(SafePeripheralHandle peripheral);

    [DllImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_manufacturer_data_get")]
    internal static extern CallResult GetManufacturerData(SafePeripheralHandle peripheral,
        nuint index,
        ref BleManufacturerData data);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_connect")]
    internal static partial CallResult ConnectPeripheral(SafePeripheralHandle peripheral);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_identifier")]
    internal static partial StringHandle GetPeripheralIdentifier(SafePeripheralHandle peripheral);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_address")]
    internal static partial StringHandle GetPeripheralAddress(SafePeripheralHandle peripheral);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_disconnect")]
    internal static partial CallResult DisconnectPeripheral(SafePeripheralHandle peripheral);

    [DllImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_notify")]
    internal static extern CallResult OnNotify(SafePeripheralHandle peripheral,
        [MarshalAs(UnmanagedType.Struct)] BleUuid service,
        [MarshalAs(UnmanagedType.Struct)] BleUuid characteristic,
        NotifyCallback callback,
        IntPtr data);

    [DllImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_write_request")]
    internal static extern CallResult WriteRequest(SafePeripheralHandle peripheral,
        [MarshalAs(UnmanagedType.Struct)] BleUuid service,
        [MarshalAs(UnmanagedType.Struct)] BleUuid characteristic,
        ref byte data,
        nuint length);

    [DllImport(SimpleBleLibraryName, EntryPoint = "simpleble_peripheral_unsubscribe")]
    internal static extern CallResult Unsubscribe(SafePeripheralHandle peripheral,
        [MarshalAs(UnmanagedType.Struct)] BleUuid service,
        [MarshalAs(UnmanagedType.Struct)] BleUuid characteristic);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_logging_set_level")]
    internal static partial void SetLogLevel(BleLogLevel level);

    [LibraryImport(SimpleBleLibraryName, EntryPoint = "simpleble_logging_set_callback")]
    internal static partial void SetLogCallback(LogCallback callback);
}