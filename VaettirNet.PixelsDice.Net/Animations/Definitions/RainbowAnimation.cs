using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class RainbowAnimation : Animation
{
    public override AnimationType Type => AnimationType.Rainbow;
    
    public uint FaceMask { get; }
    public byte Count { get; }
    public double Fade { get; }
    public double Intensity { get; }
    public byte Cycles { get; }

    public RainbowAnimation(
        int durationMs,
        uint faceMask,
        byte count,
        double fade,
        double intensity,
        byte cycles,
        AnimationFlags flags = AnimationFlags.None
    ) : base(durationMs, flags)
    {
        ArgumentValidation.ThrowIfNotUnit(fade);
        ArgumentValidation.ThrowIfNotUnit(intensity);
        ArgumentValidation.ThrowIfOutOfRange(cycles, 0, 25);

        FaceMask = faceMask;
        Count = count;
        Fade = fade;
        Intensity = intensity;
        Cycles = cycles;
    }

    public RainbowAnimation(
        TimeSpan duration,
        uint faceMask,
        byte count,
        double fade,
        double intensity,
        byte cycles,
        AnimationFlags flags = AnimationFlags.None
    ) : this(
        (int)duration.TotalMilliseconds,
        faceMask,
        count,
        fade,
        intensity,
        cycles,
        flags)
    {
    }

    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, AnimationBuffers data)
    {
        return new CombinedAnimationData<RainbowAnimationData>(shared,
            new RainbowAnimationData
            {
                FaceMask = FaceMask,
                CyclesTimes10 = (byte)(Cycles * 10),
                Intensity = (byte)(Intensity * 255),
                Count = Count,
                Fade = (byte)(Fade * 255),
            });
    }
}