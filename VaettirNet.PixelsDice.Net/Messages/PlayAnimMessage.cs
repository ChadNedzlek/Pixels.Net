using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PlayAnimMessage
{
    public MessageType Id = MessageType.PlayAnim;
    public byte Animation;
    public byte RemapFace;
    public byte LoopCount;

    public PlayAnimMessage()
    {
        Animation = 0;
        RemapFace = 0;
        LoopCount = 0;
    }
}