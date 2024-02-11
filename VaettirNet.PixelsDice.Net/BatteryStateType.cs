using System;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[Flags]
public enum BatteryStateType : byte
{
    None = 0,
    Ok = 1 << 0,
    Low = 1 << 1,
    Charging = 1 << 2,
    Done = 1 << 3,
    BadCharging = 1 << 4,
    Error = 1 << 5,
}