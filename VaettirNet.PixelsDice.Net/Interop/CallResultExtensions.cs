using System;
using System.Runtime.CompilerServices;

namespace VaettirNet.PixelsDice.Net.Interop;

public static class CallResultExtensions
{
    public static void CheckSuccess(this CallResult result,
        [CallerArgumentExpression(nameof(result))] string name = null)
    {
        var baseName = "ble method";
        name ??= "";
        int idx = name.IndexOf(nameof(NativeMethods) + '.', StringComparison.Ordinal);
        if (idx != -1)
        {
            int pi = name.IndexOf('(', idx);
            if (pi != -1) baseName = name[idx..pi];
        }

        if (result == CallResult.Failure)
            throw new Exception($"failed to call {baseName}");
    }
}