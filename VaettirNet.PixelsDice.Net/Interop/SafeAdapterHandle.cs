using System;
using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Interop;

public class SafeAdapterHandle : SafeHandle
{
    public SafeAdapterHandle() : base(IntPtr.Zero, true)
    {
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseAdapter(handle);
        return true;
    }
}