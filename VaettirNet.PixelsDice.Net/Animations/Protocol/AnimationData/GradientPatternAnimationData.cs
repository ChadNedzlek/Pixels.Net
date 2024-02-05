using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct GradientPatternAnimationData
{
    public ushort TrackOffset;
    public ushort TrackCount;
    public ushort GradientTrackOffset;
    public byte OverrideWithFace;
    private byte _overridePadding;
}