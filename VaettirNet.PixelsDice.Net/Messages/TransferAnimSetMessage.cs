using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TransferAnimSetMessage
{
    public MessageType Id = MessageType.TransferAnimSet;
    public ushort PaletteSize;
    public ushort RgbKeyFrameCount;
    public ushort RgbTrackCount;
    public ushort KeyFrameCount;
    public ushort TrackCount;

    public ushort AnimationCount;
    public ushort AnimationSize;

    public ushort ConditionCount;
    public ushort ConditionSize;
    public ushort ActionCount;
    public ushort ActionSize;
    public ushort RuleCount;

    public TransferAnimSetMessage()
    {
        PaletteSize = 0;
        RgbKeyFrameCount = 0;
        RgbTrackCount = 0;
        KeyFrameCount = 0;
        TrackCount = 0;
        AnimationCount = 0;
        AnimationSize = 0;
        ConditionCount = 0;
        ConditionSize = 0;
        ActionCount = 0;
        ActionSize = 0;
        RuleCount = 0;
    }
}