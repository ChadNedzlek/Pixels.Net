namespace VaettirNet.PixelsDice.Net;

public enum RollState : byte
{
    Unknown = 0,
    OnFace,
    Handling,
    Rolling,
    Crooked,
    Count
}