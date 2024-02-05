using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class CycleAnimation : Animation
{
    public override AnimationType Type => AnimationType.Cycle;

    public RgbTrack Track { get; }
    public byte Count { get; }
    public double Fade { get; }
    public double Intensity { get; }
    public int Cycles { get; }
    public uint FaceMask { get; }

    public CycleAnimation(
        int durationMs,
        uint faceMask,
        RgbTrack track,
        byte count,
        double fade,
        double intensity,
        int cycles,
        AnimationFlags flags = AnimationFlags.None) : base(durationMs, flags)
    {
        ArgumentValidation.ThrowIfNotUnit(fade);
        Track = track;
        Count = count;
        Fade = fade;
        Intensity = intensity;
        Cycles = cycles;
        FaceMask = faceMask;
    }
    public CycleAnimation(TimeSpan duration,
        uint faceMask,
        RgbTrack track,
        byte count,
        double fade,
        double intensity,
        int cycles,
        AnimationFlags flags = AnimationFlags.None) : this((int)duration.TotalMilliseconds, faceMask, track, count, fade, intensity, cycles, flags)
    {
    }

    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, GlobalAnimationData data)
    {
        var gradientTrack = data.StoreTrack(Track.ToProtocol(data));
        return new CombinedAnimationData<CycleAnimationData>(shared, new CycleAnimationData
        {
            Count = Count,
            Fade = (byte)(Fade * 255),
            Intensity = (byte)(Intensity * 255),
            FaceMask = FaceMask,
            CyclesTimes10 = (byte)(Cycles * 10),
            GradientTrackOffset = gradientTrack
        });
    }
}