using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BulkDataAckMessage
{
    public MessageType Id = MessageType.BulkDataAck;
    public ushort Offset;

    public BulkDataAckMessage()
    {
        Offset = 0;
    }
}