using System;
using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Interop;

public class StringHandle : SafeHandle
{
    private string _value;

    public StringHandle() : base(IntPtr.Zero, true)
    {
    }

    public string Value
    {
        get
        {
            if (_value == null) _value = Marshal.PtrToStringAnsi(handle);

            return _value;
        }
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleaseHandle(handle);
        return true;
    }
}