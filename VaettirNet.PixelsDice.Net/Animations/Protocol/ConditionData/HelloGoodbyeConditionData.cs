using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HelloGoodbyeConditionData
{
    public ConditionType Type = ConditionType.HelloGoodbye;
    public HelloGoodbyeType Condition;
    private byte _padding2;
    private byte _padding3;

    public HelloGoodbyeConditionData()
    {
        Condition = HelloGoodbyeType.None;
        _padding2 = 0;
        _padding3 = 0;
    }
}