using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AnimationRuleData
{
    public ushort ConditionIndex;
    public ushort ActionIndex;
    public ushort ActionCount;
    private ushort _padding;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AnimationBehaviorData
{
    public ushort RuleIndex;
    public ushort RuleCount;
}