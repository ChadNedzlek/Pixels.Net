using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class GradientAnimation : Animation
{
    public override AnimationType Type => AnimationType.Gradient;
    public uint FaceMask { get; }
    public RgbTrack Track { get; }

    public GradientAnimation(
        int durationMs,
        uint faceMask,
        RgbTrack track,
        AnimationFlags flags = AnimationFlags.None
    ) : base(durationMs, flags)
    {
        FaceMask = faceMask;
        Track = track;
    }

    public GradientAnimation(
        TimeSpan duration,
        uint faceMask,
        RgbTrack track,
        AnimationFlags flags = AnimationFlags.None
    ) : this((int)duration.TotalMilliseconds, faceMask, track, flags)
    {
    }
    
    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, ref AnimationBuffers data)
    {
        ushort track = data.StoreTrack(Track.ToProtocol(ref data));
        return new CombinedAnimationData<GradientAnimationData>(shared, new GradientAnimationData
        {
            FaceMask = FaceMask,
            GradientTrackOffset = track
        });
    }
}