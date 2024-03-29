﻿using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BlinkMessage
{
    public MessageType Id = MessageType.Blink;
    public byte Count;
    public short DurationMs;
    public int Color;
    public int Faces;
    public byte Fade;
    [MarshalAs(UnmanagedType.U1)] public bool Loop;

    public BlinkMessage()
    {
    }
}