using System;
using System.Runtime.CompilerServices;

namespace VaettirNet.PixelsDice.Net;

internal static class ArgumentValidation
{
    public static void ThrowIfOutOfRange<T>(T value, T min, T max, [CallerArgumentExpression(nameof(value))]string parameterName = null) where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, min, parameterName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, max, parameterName);
    }
    
    public static void ThrowIfNotUnit(double value, [CallerArgumentExpression(nameof(value))]string parameterName = null) => ThrowIfOutOfRange(value, 0, 1, parameterName);
}