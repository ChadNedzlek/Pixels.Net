using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HandlingConditionData
{
    public ConditionType Type = ConditionType.Handling;
    private byte _padding1;
    private byte _padding2;
    private byte _padding3;

    public HandlingConditionData()
    {
        _padding1 = 0;
        _padding2 = 0;
        _padding3 = 0;
    }
}