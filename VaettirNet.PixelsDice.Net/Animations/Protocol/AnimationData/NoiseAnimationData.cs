using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.Types;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct NoiseAnimationData
{
    public ushort OverallGradiantTrackOffset;
    public ushort IndividualGradientTrackOffset;
    public ushort BlinkFrequencyTimes1000;
    public ushort BlinkFrequencyVarTimes1000;
    public ushort BlinkDurationMs;
    public byte Fade;
    public NoiseColorOverrideType OverrideType;
    public ushort OverallGradientColorVar;
}