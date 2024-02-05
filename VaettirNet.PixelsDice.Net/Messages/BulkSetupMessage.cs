using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BulkSetupMessage
{
    public MessageType Id = MessageType.BulkSetup;
    public ushort Size;

    public BulkSetupMessage()
    {
        Size = 0;
    }
}