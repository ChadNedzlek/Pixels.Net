using System;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[Flags]
internal enum HelloGoodbyeType : byte
{
    None = 0,
    Hello = 1,
    Goodbye = 2,
}