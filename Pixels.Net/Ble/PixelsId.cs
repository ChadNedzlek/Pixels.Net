using Pixels.Net.Interop;

namespace Pixels.Net.Ble;

internal static class PixelsId
{
    internal const string ServiceId = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    internal const string NotifyCharacteristicId = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    internal const string WriteCharacteristic = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
    internal static readonly BleUuid ServiceUuid = new(ServiceId);
    internal static readonly BleUuid NotifyCharacteristicUuid = new(NotifyCharacteristicId);
    internal static readonly BleUuid WriteCharacteristicUuid = new(WriteCharacteristic);
}