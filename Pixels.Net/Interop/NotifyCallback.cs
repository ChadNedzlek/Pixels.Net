using System;

namespace Pixels.Net.Interop;

internal delegate void NotifyCallback(BleUuid service, BleUuid characteristic, IntPtr data, nuint dataLength, IntPtr userdata);