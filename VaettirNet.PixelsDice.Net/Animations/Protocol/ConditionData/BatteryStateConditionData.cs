using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BatteryStateConditionData
{
    public ConditionType Type = ConditionType.BatteryState;
    public BatteryStateType Condition;
    public ushort RepeatPeriodMs;

    public BatteryStateConditionData()
    {
        Condition = BatteryStateType.None;
        RepeatPeriodMs = 0;
    }
}