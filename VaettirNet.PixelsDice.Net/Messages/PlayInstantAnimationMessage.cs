using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PlayInstantAnimationMessage
{
    public MessageType Id = MessageType.PlayInstantAnim;
    public byte Animation;
    public byte FaceIndex;
    public byte LoopCount;

    public PlayInstantAnimationMessage()
    {
        Animation = 0;
        FaceIndex = 0;
        LoopCount = 0;
    }
}