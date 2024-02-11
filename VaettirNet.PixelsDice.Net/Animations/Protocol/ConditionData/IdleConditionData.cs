using System.Runtime.InteropServices;

// Many padding fields for struct alignment 
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct IdleConditionData
{
    public ConditionType Type = ConditionType.Idle;
    private byte _padding;
    public ushort RepeatPeriodMs;

    public IdleConditionData()
    {
        RepeatPeriodMs = 0;
    }
}