using System;

namespace Pixels.Net.Ble;

internal delegate void OnNotifyCallback(ReadOnlySpan<byte> data);