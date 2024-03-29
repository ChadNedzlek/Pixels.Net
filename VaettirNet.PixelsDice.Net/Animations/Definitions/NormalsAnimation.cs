using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class NormalsAnimation : Animation
{
    public override AnimationType Type => AnimationType.Normals;

    public RgbTrack AngleTrack { get; }
    public RgbTrack AxisTrack { get; }
    public RgbTrack TimeTrack { get; }
    public double Fade { get; }
    public double AxisOffset { get; }
    public double AxisScale { get; }
    public double AxisScrollSpeed { get; }
    public double AngleScrollSpeed { get; }
    public double MainGradientColorVariance { get; }
    public NormalsColorOverrideType OverrideType { get; }

    public NormalsAnimation(
        int durationMs,
        RgbTrack angleTrack,
        RgbTrack axisTrack,
        RgbTrack timeTrack,
        double fade,
        double axisOffset,
        double axisScale,
        double axisScrollSpeed,
        double angleScrollSpeed,
        double mainGradientColorVariance,
        NormalsColorOverrideType overrideType,
        AnimationFlags flags = AnimationFlags.None
    ) : base(durationMs, flags)
    {
        ArgumentValidation.ThrowIfNotUnit(fade);
        ArgumentValidation.ThrowIfNotUnit(mainGradientColorVariance);
        ArgumentValidation.ThrowIfOutOfRange(axisScale, short.MinValue / 1000.0, short.MaxValue / 1000.0);
        ArgumentValidation.ThrowIfOutOfRange(axisOffset, short.MinValue / 1000.0, short.MaxValue / 1000.02);
        ArgumentValidation.ThrowIfOutOfRange(axisScrollSpeed, short.MinValue / 1000.0, short.MaxValue / 1000.0);
        ArgumentValidation.ThrowIfOutOfRange(angleScrollSpeed, short.MinValue / 1000.0, short.MaxValue / 1000.0);
        
        AngleTrack = angleTrack;
        AxisTrack = axisTrack;
        TimeTrack = timeTrack;
        Fade = fade;
        AxisOffset = axisOffset;
        AxisScale = axisScale;
        AxisScrollSpeed = axisScrollSpeed;
        AngleScrollSpeed = angleScrollSpeed;
        MainGradientColorVariance = mainGradientColorVariance;
        OverrideType = overrideType;
    }

    public NormalsAnimation(
        TimeSpan duration,
        RgbTrack angleTrack,
        RgbTrack axisTrack,
        RgbTrack timeTrack,
        double fade,
        double axisOffset,
        double axisScale,
        double axisScrollSpeed,
        double angleScrollSpeed,
        double mainGradientColorVariance,
        NormalsColorOverrideType overrideType,
        AnimationFlags flags = AnimationFlags.None
    ) : this(
        (int)duration.TotalMilliseconds,
        angleTrack,
        axisTrack,
        timeTrack,
        fade,
        axisOffset,
        axisScale,
        axisScrollSpeed,
        angleScrollSpeed,
        mainGradientColorVariance,
        overrideType,
        flags)
    {
    }

    
    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, ref AnimationBuffers data)
    {
        ushort angleOffset = data.StoreTrack(AngleTrack.ToProtocol(ref data));
        ushort axisOffset = data.StoreTrack(AxisTrack.ToProtocol(ref data));
        ushort timeOffset = data.StoreTrack(TimeTrack.ToProtocol(ref data));
        return new CombinedAnimationData<NormalsAnimationData>(shared, new NormalsAnimationData
        {
            Fade = (byte)(Fade * 255),
            OverrideType = OverrideType,
            AxisOffsetTimes1000 = (short)(AxisOffset * 1000),
            AxisScaleTimes1000 = (short)(AxisScale * 1000),
            AxisScrollSpeedTimes1000 = (short)(AxisScrollSpeed * 1000),
            AngleScrollSpeedTimes1000 = (short)(AngleScrollSpeed * 1000),
            MainGradientColorVar = (ushort)(MainGradientColorVariance * 1000),
            GradientAlongAngle = angleOffset,
            GradientAlongAxis = axisOffset,
            GradientOverTime = timeOffset,
        });
    }
}