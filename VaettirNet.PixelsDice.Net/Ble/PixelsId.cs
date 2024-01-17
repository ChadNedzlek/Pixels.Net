using VaettirNet.PixelsDice.Net.Interop;

namespace VaettirNet.PixelsDice.Net.Ble;

internal static class PixelsId
{
    internal const string InfoServiceId = "0000180a-0000-1000-8000-00805f9b34fb";
    internal const string PixelsServiceId = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    internal const string NotifyCharacteristicId = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    internal const string WriteCharacteristic = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
    internal static readonly BleUuid PixelsServiceUuid = new(PixelsServiceId);
    internal static readonly BleUuid NotifyCharacteristicUuid = new(NotifyCharacteristicId);
    internal static readonly BleUuid WriteCharacteristicUuid = new(WriteCharacteristic);
}