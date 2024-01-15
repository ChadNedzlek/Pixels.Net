using System;
using System.Runtime.InteropServices;

namespace Pixels.Net.Interop;

public class SafeAdapterHandle : SafeHandle
{
    public SafeAdapterHandle() : base(IntPtr.Zero, true)
    {
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseAdapter(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}