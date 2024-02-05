using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct KeyFrame
{
    public ushort Data;

    public byte Intensity => (byte)(Data & 0x7F);
    public int DurationMs => (Data >> 7) * 1000 / 50;

    public static KeyFrame Create(byte intensity, uint durationMs)
    {
        return new KeyFrame { Data = (ushort)((uint)(intensity & 0x7F) | ((durationMs * 50 / 1000) << 7)) };
    }
}