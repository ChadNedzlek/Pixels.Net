using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BlinkIdAnimationData
{
    public byte FramesPerBlink;
    public byte Brightness;
}