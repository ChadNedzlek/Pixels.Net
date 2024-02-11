using System.Collections.Immutable;
using System.Linq;
using VaettirNet.PixelsDice.Net.Animations.Actions;
using VaettirNet.PixelsDice.Net.Animations.Conditions;
using VaettirNet.PixelsDice.Net.Animations.Protocol;

namespace VaettirNet.PixelsDice.Net.Animations;

public class AnimationRule
{
    public Condition Condition { get; }
    public ImmutableList<DieAction> Actions { get; }
    
    public AnimationRule(Condition condition, ImmutableList<DieAction> actions)
    {
        Condition = condition;
        Actions = actions;
    }

    internal ushort ToProtocol(ref AnimationBuffers data)
    {
        int actionIndex = data.ActionBuffer.Count;
        foreach (var action in Actions)
        {
            action.ToProtocol(ref data);
        }

        ushort conditionIndex = Condition.ToProtocol(ref data);

        var protocol = new AnimationRuleData
        {
            ActionIndex = (ushort)actionIndex,
            ActionCount = (ushort)Actions.Count,
            ConditionIndex = conditionIndex,
        };

        return data.StoreRule(protocol);
    }
}