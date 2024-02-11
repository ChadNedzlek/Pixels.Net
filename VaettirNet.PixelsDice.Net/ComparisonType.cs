namespace VaettirNet.PixelsDice.Net;

public enum ComparisonType : byte
{
    LessThan = 1,
    Equal = 2,
    GreaterThan = 4,
    LessThanOrEqual = LessThan | Equal,
    GreaterThanOrEqual = GreaterThan | Equal,
}