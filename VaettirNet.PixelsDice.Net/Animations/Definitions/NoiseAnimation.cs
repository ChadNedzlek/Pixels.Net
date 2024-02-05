using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;
using VaettirNet.PixelsDice.Net.Animations.Protocol.Types;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class NoiseAnimation : Animation
{
    public override AnimationType Type => AnimationType.Noise;
    
    public RgbTrack OverallGradient { get; }
    public RgbTrack IndividualGradient { get; }
    public double BlinksPerSecond { get; }
    public double BlinksPerSecondsVariance { get; }
    public int BlinkDurationMs { get; }
    public double Fade { get; }
    public NoiseColorOverrideType ColorType { get; }
    public double OverallColorVariance { get; }

    public NoiseAnimation(
        int durationMs,
        RgbTrack overallGradient,
        RgbTrack individualGradient,
        double blinksPerSecond,
        double blinksPerSecondsVariance,
        int blinkDurationMs,
        double fade,
        NoiseColorOverrideType colorType,
        double overallColorVariance,
        AnimationFlags flags = AnimationFlags.None)
        : base(durationMs, flags)
    {
        ArgumentValidation.ThrowIfNotUnit(fade);
        ArgumentValidation.ThrowIfNotUnit(overallColorVariance);
        ArgumentValidation.ThrowIfOutOfRange(blinksPerSecond, 0, 65);
        
        ArgumentOutOfRangeException.ThrowIfNegative(blinksPerSecondsVariance);

        if (blinksPerSecondsVariance >= blinksPerSecond)
        {
            throw new ArgumentOutOfRangeException(nameof(blinksPerSecondsVariance),
                "variance must be less than blinksPerSecond");
        }
        
        OverallGradient = overallGradient;
        IndividualGradient = individualGradient;
        BlinksPerSecond = blinksPerSecond;
        BlinksPerSecondsVariance = blinksPerSecondsVariance;
        Fade = fade;
        ColorType = colorType;
        OverallColorVariance = overallColorVariance;
        BlinkDurationMs = blinkDurationMs;
    }

    public NoiseAnimation(
        int durationMs,
        RgbTrack overallGradient,
        RgbTrack individualGradient,
        double blinksPerSecond,
        int blinkDuration,
        double fade,
        NoiseColorOverrideType colorType,
        double overallColorVariance,
        AnimationFlags flags = AnimationFlags.None)
        : this(durationMs, overallGradient, individualGradient, blinksPerSecond, 0, blinkDuration, fade, colorType, overallColorVariance, flags)
    {
    }

    public NoiseAnimation(
        TimeSpan duration,
        RgbTrack overallGradient,
        RgbTrack individualGradient,
        TimeSpan blinkSpeed,
        TimeSpan blinkSpeedVariance,
        TimeSpan blinkDuration,
        double fade,
        NoiseColorOverrideType colorType,
        double overallColorVariance,
        AnimationFlags flags = AnimationFlags.None)
        : this(
            (int)duration.TotalMilliseconds,
            overallGradient,
            individualGradient,
            1.0 / blinkSpeed.TotalSeconds,
            blinkSpeedVariance.Ticks == 0 ? 0 : 1.0 / blinkSpeedVariance.TotalSeconds,
            (int)blinkDuration.TotalMilliseconds,
            fade,
            colorType,
            overallColorVariance,
            flags)
    {
    }

    public NoiseAnimation(
        TimeSpan duration,
        RgbTrack overallGradient,
        RgbTrack individualGradient,
        TimeSpan blinkSpeed,
        TimeSpan blinkDuration,
        double fade,
        NoiseColorOverrideType colorType,
        double overallColorVariance,
        AnimationFlags flags = AnimationFlags.None)
        : this(duration, overallGradient, individualGradient, blinkSpeed, TimeSpan.Zero, blinkDuration, fade, colorType, overallColorVariance, flags)
    {
    }

    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, GlobalAnimationData data)
    {
        ushort overallTrack = data.StoreTrack(OverallGradient.ToProtocol(data));
        ushort individualTrack = data.StoreTrack(IndividualGradient.ToProtocol(data));
        return new CombinedAnimationData<NoiseAnimationData>(
            shared,
            new NoiseAnimationData
            {
                Fade = (byte)(255 * Fade),
                OverrideType = ColorType,
                BlinkDurationMs = (ushort)BlinkDurationMs,
                BlinkFrequencyTimes1000 = (ushort)(BlinksPerSecond * 1000),
                BlinkFrequencyVarTimes1000 = (ushort)(BlinksPerSecondsVariance * 1000),
                OverallGradiantTrackOffset = overallTrack,
                IndividualGradientTrackOffset = individualTrack,
                OverallGradientColorVar = (ushort)(OverallColorVariance * 1000),
            }
        );
    }
}