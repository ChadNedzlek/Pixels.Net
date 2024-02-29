using System;

namespace VaettirNet.PixelsDice.Net;

internal static class PixelsId
{
    private const string InfoServiceId = "0000180a-0000-1000-8000-00805f9b34fb";
    private const string PixelsServiceId = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    private const string NotifyCharacteristicId = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    private const string WriteCharacteristic = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";
    internal static readonly Guid InfoServiceUuid = Guid.Parse(InfoServiceId);
    internal static readonly Guid PixelsServiceUuid = Guid.Parse(PixelsServiceId);
    internal static readonly Guid NotifyCharacteristicUuid = Guid.Parse(NotifyCharacteristicId);
    internal static readonly Guid WriteCharacteristicUuid = Guid.Parse(WriteCharacteristic);
}