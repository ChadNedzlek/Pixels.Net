using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations;

internal static class SerializationUtils
{
    public static int RoundTo4(int value)
    {
        return (value + 3) / 4 * 4;
    }

    public static (int paddedSize, int padding) PadTo4(int value)
    {
        var padded = (value + 3) / 4 * 4;
        return (padded, padded - value);
    }

    public static void WriteArray<T>(ref Span<byte> buffer, IEnumerable<T> arr) where T : struct
    {
        foreach (var item in arr)
        {
            Write(ref buffer, item);
        }
    }

    public static void Write<T>(ref Span<byte> buffer, T thing) where T : struct
    {
        MemoryMarshal.Write(buffer, thing);
        buffer = buffer[Unsafe.SizeOf<T>()..];
    }
}