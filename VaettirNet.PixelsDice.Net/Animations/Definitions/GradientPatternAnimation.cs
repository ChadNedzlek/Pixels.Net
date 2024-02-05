using System;
using System.Collections.Immutable;
using System.Linq;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class GradientPatternAnimation : Animation
{
    public override AnimationType Type => AnimationType.GradientPattern;
    public ImmutableList<Track> Tracks { get; }
    public RgbTrack ColorTrack { get; }
    public byte OverrideWithFace { get; }

    public GradientPatternAnimation(
        int durationMs,
        ImmutableList<Track> tracks,
        RgbTrack colorTrack,
        byte overrideWithFace,
        AnimationFlags flags = AnimationFlags.None
    ) : base(durationMs, flags)
    {
        Tracks = tracks;
        ColorTrack = colorTrack;
        OverrideWithFace = overrideWithFace;
    }

    public GradientPatternAnimation(
        TimeSpan duration,
        ImmutableList<Track> tracks,
        RgbTrack colorTrack,
        byte overrideWithFace,
        AnimationFlags flags = AnimationFlags.None
    ) : base(
        (int)duration.TotalMilliseconds,
        flags)
    {
        Tracks = tracks;
        ColorTrack = colorTrack;
        OverrideWithFace = overrideWithFace;
    }

    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, GlobalAnimationData data)
    {
        ushort trackOffset = data.StoreTracks(Tracks.Select(t => t.ToProtocol(data)));
        ushort gradientTrack = data.StoreTrack(ColorTrack.ToProtocol(data));
        return new CombinedAnimationData<GradientPatternAnimationData>(shared,
            new GradientPatternAnimationData
            {
                TrackOffset = trackOffset,
                TrackCount = (ushort)Tracks.Count,
                GradientTrackOffset = gradientTrack,
                OverrideWithFace = OverrideWithFace,
            });
    }
}