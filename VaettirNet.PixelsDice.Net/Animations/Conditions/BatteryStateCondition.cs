using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public partial class BatteryStateCondition : Condition
{
    public BatteryStateCondition(BatteryStateType state, ushort repeatPeriodMs)
    {
        State = state;
        RepeatPeriodMs = repeatPeriodMs;
    }

    public BatteryStateCondition(BatteryStateType state, TimeSpan repeatPeriodMs) : this(state,
        checked((ushort)repeatPeriodMs.TotalMilliseconds))
    {
    }

    public BatteryStateType State { get; }
    public ushort RepeatPeriodMs { get; }

    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<BatteryStateConditionData>(new BatteryStateConditionData
            { Condition = State, RepeatPeriodMs = RepeatPeriodMs });
    }
}