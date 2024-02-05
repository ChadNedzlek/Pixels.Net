using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Track
{
    public ushort KeyFrameOffset;
    public byte KeyFrameCount;
    private byte _padding;
    public uint LedMask;
}