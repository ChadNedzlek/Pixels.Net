using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct CycleAnimationData
{
    public uint FaceMask;
    public byte Count;
    public byte Fade;
    public byte Intensity;
    public byte CyclesTimes10;
    public ushort GradientTrackOffset;
}