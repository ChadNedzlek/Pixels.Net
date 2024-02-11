using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct FaceCompareConditionData
{
    public ConditionType Type = ConditionType.FaceCompare;
    public byte Face;
    public ComparisonType Comparison;
    private byte _padding;

    public FaceCompareConditionData()
    {
        Face = 0;
        Comparison = (ComparisonType)0;
        _padding = 0;
    }
}