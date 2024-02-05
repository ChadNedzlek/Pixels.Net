using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct KeyFramedAnimationData
{
    public ushort TrackOffset;
    public ushort TrackCount;
}