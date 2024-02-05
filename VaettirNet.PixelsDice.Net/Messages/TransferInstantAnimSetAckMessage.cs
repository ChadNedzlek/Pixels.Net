using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TransferInstantAnimSetAckMessage
{
    public MessageType Id = MessageType.TransferInstantAnimSetAck;
    public TransferInstantAnimSetAckType Type;

    public TransferInstantAnimSetAckMessage()
    {
    }
}