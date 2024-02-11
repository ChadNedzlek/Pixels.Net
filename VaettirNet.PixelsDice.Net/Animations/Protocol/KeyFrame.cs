using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct KeyFrame
{
    public ushort Data;

    public byte Intensity => (byte)(Data & 0x7F);
    public int TimeMs => (Data >> 7) * 2;

    public static KeyFrame Create(byte intensity, uint timeMs)
    {
        return new KeyFrame { Data = (ushort)((ushort)(intensity & 0x7F) | ((timeMs / 2) << 7)) };
    }
}