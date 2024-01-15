using System;

namespace VaettirNet.PixelsDice.Net.Ble;

internal delegate void OnNotifyCallback(ReadOnlySpan<byte> data);