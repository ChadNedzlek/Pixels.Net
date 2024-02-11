namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

internal enum ConditionType : byte
{
    Unknown = 0,
    HelloGoodbye,
    Handling,
    Rolling,
    FaceCompare,
    Crooked,
    ConnectionState,
    BatteryState,
    Idle,
    Rolled,
}