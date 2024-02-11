using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public class RollingCondition : Condition
{
    public ushort RepeatPeriodMs { get; }
    
    public RollingCondition(ushort repeatPeriodMs)
    {
        RepeatPeriodMs = repeatPeriodMs;
    }

    public RollingCondition(TimeSpan repeatPeriodMs) : this(checked((ushort)repeatPeriodMs.TotalMilliseconds))
    {
    }
    
    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<RollingConditionData>(new RollingConditionData
        {
            RepeatPeriodMs = RepeatPeriodMs
        });
    }
}