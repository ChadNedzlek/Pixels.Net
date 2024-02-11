using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public class IdleCondition : Condition
{
    public ushort RepeatPeriodMs { get; }
    
    public IdleCondition(ushort repeatPeriodMs)
    {
        RepeatPeriodMs = repeatPeriodMs;
    }

    public IdleCondition(TimeSpan repeatPeriodMs) : this(checked((ushort)repeatPeriodMs.TotalMilliseconds))
    {
    }
    
    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<IdleConditionData>(new IdleConditionData
        {
            RepeatPeriodMs = RepeatPeriodMs
        });
    }
}