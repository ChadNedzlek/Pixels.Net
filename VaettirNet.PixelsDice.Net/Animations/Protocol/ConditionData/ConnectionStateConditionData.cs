using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ConnectionStateConditionData
{
    public ConditionType Type = ConditionType.ConnectionState;
    public ConnectionStateType Condition;
    private byte _padding2;
    private byte _padding3;

    public ConnectionStateConditionData()
    {
        Condition = ConnectionStateType.None;
        _padding2 = 0;
        _padding3 = 0;
    }
}