using System;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[Flags]
internal enum ConnectionStateType : byte
{
    None = 0,
    Connected = 1,
    Disconnected = 2,
}