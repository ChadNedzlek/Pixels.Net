using System;

namespace VaettirNet.PixelsDice.Net.Interop;

internal delegate void NotifyCallback(BleUuid service, BleUuid characteristic, IntPtr data, nuint dataLength, IntPtr userdata);