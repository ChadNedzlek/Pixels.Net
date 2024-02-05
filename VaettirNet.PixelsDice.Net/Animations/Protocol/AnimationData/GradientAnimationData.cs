using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct GradientAnimationData
{
    public uint FaceMask;
    public ushort GradientTrackOffset;
    private ushort _gradientPadding;
}