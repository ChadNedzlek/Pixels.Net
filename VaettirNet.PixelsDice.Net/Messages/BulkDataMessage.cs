using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BulkDataMessage
{
    public MessageType Id = MessageType.BulkData;
    public byte Size;
    public ushort Offset;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public unsafe fixed byte Data[100];

    public BulkDataMessage()
    {
        Size = 0;
        Offset = 0;
    }
}