﻿using System.Runtime.InteropServices;

namespace Pixels.Net.Messages;

[StructLayout(LayoutKind.Sequential)]
internal struct RollStateMessage
{
    public byte Id;
    public RollState RollState;
    public byte CurrentFace;
}