using VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public class HandlingCondition : Condition
{
    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<HandlingConditionData>(new HandlingConditionData());
    }
}