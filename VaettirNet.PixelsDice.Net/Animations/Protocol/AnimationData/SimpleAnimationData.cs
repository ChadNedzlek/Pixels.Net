using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SimpleAnimationData
{
    public uint FaceMask;
    public ushort ColorIndex;
    public byte Count;
    public byte Fade;
}