using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PlayAnimationMessage
{
    public MessageType Id = MessageType.PlayAnim;
    public byte Animation;
    public byte FaceIndex;
    public byte LoopCount;

    public PlayAnimationMessage()
    {
        Animation = 0;
        FaceIndex = 0;
        LoopCount = 0;
    }
}