using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RemoteActionMessage
{
    public MessageType Id = MessageType.RemoteAction;
    public ushort ActionId;

    public RemoteActionMessage()
    {
        ActionId = 0;
    }
}