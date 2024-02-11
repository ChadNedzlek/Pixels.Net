using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public class HelloGoodbyeCondition : Condition
{
    public bool OnHello { get; }
    public bool OnGoodbye { get; }

    public HelloGoodbyeCondition(bool onHello, bool onGoodbye)
    {
        if (!onHello && !onGoodbye)
        {
            throw new ArgumentException("At least one of hello/goodbye must be set");
        }

        OnHello = onHello;
        OnGoodbye = onGoodbye;
    }

    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<HelloGoodbyeConditionData>(new HelloGoodbyeConditionData
        {
            Condition = (OnHello ? HelloGoodbyeType.Hello : 0) | (OnGoodbye ? HelloGoodbyeType.Goodbye : 0),
        });
    }
}