using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TransferAnimSetAckMessage
{
    public MessageType Id = MessageType.TransferInstantAnimSetAck;
    public byte Result;

    public TransferAnimSetAckMessage()
    {
    }
}