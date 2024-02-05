using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RgbKeyFrame
{
    public ushort Data;

    public byte ColorIndex => (byte)(Data & 0x7F);
    public int DurationMs => (Data >> 7) * 1000 / 50;

    public static RgbKeyFrame Create(byte colorIndex, uint durationMs)
    {
        return new RgbKeyFrame { Data = (ushort)((uint)(colorIndex & 0x7F) | ((durationMs * 50 / 1000) << 7)) };
    }
}