using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RollingConditionData
{
    public ConditionType Type = ConditionType.Rolling;
    private byte _padding;
    public ushort RepeatPeriodMs;

    public RollingConditionData()
    {
        _padding = 0;
        RepeatPeriodMs = 0;
    }
}