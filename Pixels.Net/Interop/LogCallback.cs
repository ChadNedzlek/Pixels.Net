using System;

namespace Pixels.Net.Interop;

internal delegate void LogCallback(BleLogLevel level, IntPtr module, IntPtr file, uint line, IntPtr function, IntPtr message);