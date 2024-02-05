namespace VaettirNet.PixelsDice.Net.Messages;

internal enum TransferInstantAnimSetAckType : byte
{
    Download = 0,
    UpToDate,
    NoMemory,
}