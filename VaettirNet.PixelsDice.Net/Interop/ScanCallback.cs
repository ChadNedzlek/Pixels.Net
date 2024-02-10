using System;

namespace VaettirNet.PixelsDice.Net.Interop;

internal delegate void ScanCallback(IntPtr adapter, IntPtr peripheral, IntPtr data);

internal delegate void ConnectionCallback(IntPtr peripheral, IntPtr userData);