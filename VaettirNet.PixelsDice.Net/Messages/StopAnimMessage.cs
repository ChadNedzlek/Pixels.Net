using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct StopAnimMessage
{
    public MessageType Id = MessageType.StopAnim;
    public byte Animation;
    public byte RemapFace;

    public StopAnimMessage()
    {
        Animation = 0;
        RemapFace = 0;
    }
}