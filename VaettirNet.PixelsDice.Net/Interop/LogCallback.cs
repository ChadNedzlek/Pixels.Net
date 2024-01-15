using System;

namespace VaettirNet.PixelsDice.Net.Interop;

internal delegate void LogCallback(BleLogLevel level, IntPtr module, IntPtr file, uint line, IntPtr function, IntPtr message);