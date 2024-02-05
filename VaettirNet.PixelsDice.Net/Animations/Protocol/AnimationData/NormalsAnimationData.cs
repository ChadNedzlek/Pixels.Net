using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct NormalsAnimationData
{
    public ushort GradientOverTime;
    public ushort GradientAlongAxis;
    public ushort GradientAlongAngle;
    public short AxisScaleTimes1000;
    public short AxisOffsetTimes1000;
    public short AxisScrollSpeedTimes1000;
    public short AngleScrollSpeedTimes1000;
    public byte Fade;
    public NormalsColorOverrideType OverrideType;
    public ushort MainGradientColorVar;
}