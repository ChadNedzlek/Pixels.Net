using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct WhoAmIMessage
{
    public MessageType Id = MessageType.WhoAreYou;

    public WhoAmIMessage()
    {
    }
}