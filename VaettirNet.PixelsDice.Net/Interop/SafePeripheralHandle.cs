using System;
using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Interop;

public class SafePeripheralHandle : SafeHandle
{
    public SafePeripheralHandle() : base(IntPtr.Zero, true)
    {
    }

    public SafePeripheralHandle(IntPtr handle) : base(IntPtr.Zero, true)
    {
        SetHandle(handle);
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleasePeripheral(handle);
        return true;
    }
}