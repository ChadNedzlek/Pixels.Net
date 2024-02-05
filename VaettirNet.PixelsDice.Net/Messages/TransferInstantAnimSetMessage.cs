using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TransferInstantAnimSetMessage
{
    public MessageType Id = MessageType.TransferInstantAnimSet;
    public ushort PaletteSize;
    public ushort RgbKeyFrameCount;
    public ushort RgbTrackCount;
    public ushort KeyFrameCount;
    public ushort TrackCount;

    public ushort AnimationCount;
    public ushort AnimationSize;

    public uint Hash;

    public TransferInstantAnimSetMessage()
    {
        PaletteSize = 0;
        RgbKeyFrameCount = 0;
        RgbTrackCount = 0;
        KeyFrameCount = 0;
        TrackCount = 0;
        AnimationCount = 0;
        AnimationSize = 0;
        Hash = 0;
    }
}