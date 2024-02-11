using VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public class CrookedCondition : Condition
{
    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<CrookedConditionData>(new CrookedConditionData());
    }
}