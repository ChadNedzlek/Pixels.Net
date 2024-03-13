using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class WormAnimation : Animation
{
    public override AnimationType Type => AnimationType.Worm;
    public uint FaceMask { get; }
    public RgbTrack Track { get; }
    public byte Count { get; }
    public double Fade { get; }
    public double Intensity { get; }
    public int Cycles { get; }

    public WormAnimation(
        int durationMs,
        uint faceMask,
        RgbTrack track,
        byte count,
        double fade,
        double intensity,
        int cycles,
        AnimationFlags flags = AnimationFlags.None
    ) : base(durationMs, flags)
    {
        ArgumentValidation.ThrowIfNotUnit(fade);
        ArgumentValidation.ThrowIfNotUnit(intensity);
        ArgumentValidation.ThrowIfOutOfRange(cycles, 0, 25);
        
        FaceMask = faceMask;
        Track = track;
        Count = count;
        Fade = fade;
        Intensity = intensity;
        Cycles = cycles;
    }

    public WormAnimation(TimeSpan duration, uint faceMask, RgbTrack track, byte count, double fade, double intensity, int cycles, AnimationFlags flags = AnimationFlags.None) : this(
        (int)duration.TotalMilliseconds,
        faceMask,
        track,
        count,
        fade,
        intensity,
        cycles,
        flags)
    {
    }
    
    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, ref AnimationBuffers data)
    {
        ushort trackOffset = data.StoreTrack(Track.ToProtocol(ref data));
        return new CombinedAnimationData<WormAnimationData>(shared, new WormAnimationData
        {
            FaceMask = FaceMask,
            Count = Count,
            CyclesTimes10 = (byte)(Cycles * 10),
            Fade = (byte)(Fade * 255),
            GradientTrackOffset = trackOffset,
            Intensity = (byte)(Intensity * 255),
        });
    }
}
