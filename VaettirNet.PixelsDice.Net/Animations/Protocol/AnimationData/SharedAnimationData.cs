using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SharedAnimationData
{
    public AnimationType Type;
    public AnimationFlags Flags;
    public ushort Duration;
}