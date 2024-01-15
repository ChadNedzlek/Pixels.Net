﻿using System;
using System.Runtime.InteropServices;

namespace Pixels.Net.Interop;

public class SafePeripheralHandle : SafeHandle
{
    public SafePeripheralHandle() : base(IntPtr.Zero, true)
    {
    }
    
    public SafePeripheralHandle(IntPtr handle) : base(IntPtr.Zero, true)
    {
        SetHandle(handle);
    }

    protected override bool ReleaseHandle()
    {
        NativeMethods.ReleasePeripheral(handle);
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;
}