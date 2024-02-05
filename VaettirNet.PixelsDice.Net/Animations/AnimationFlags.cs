using System;

namespace VaettirNet.PixelsDice.Net.Animations;

[Flags]
public enum AnimationFlags : byte
{
    None = 0,
    Traveling = 1,
    UseLedIndices = 2,
}