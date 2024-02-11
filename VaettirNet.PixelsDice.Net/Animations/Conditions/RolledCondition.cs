using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public class RolledCondition : Condition
{
    public uint FaceMask { get; }
    
    public RolledCondition(uint faceMask)
    {
        FaceMask = faceMask;
    }
    
    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<RolledConditionData>(new RolledConditionData { FaceMask = FaceMask });
    }
}

public class FaceCompareCondition : Condition
{
    public byte FaceValue { get; }
    public ComparisonType Comparison { get; }

    public FaceCompareCondition(byte faceValue, ComparisonType comparison)
    {
        FaceValue = faceValue;
        Comparison = comparison;
    }

    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<FaceCompareConditionData>(new FaceCompareConditionData
        {
            Face = (byte)(FaceValue - 1),
            Comparison = Comparison,
        });
    }
}