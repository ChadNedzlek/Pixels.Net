using System;

namespace Pixels.Net.Interop;

internal delegate void ScanCallback(IntPtr adapter, IntPtr peripheral, IntPtr data);