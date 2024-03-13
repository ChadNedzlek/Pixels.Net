using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class GradientPatternAnimation : Animation
{
    public override AnimationType Type => AnimationType.GradientPattern;
    public ImmutableList<Track> Tracks { get; }
    public RgbTrack ColorTrack { get; }
    public bool OverrideWithFace { get; }

    public GradientPatternAnimation(
        int durationMs,
        ImmutableList<Track> tracks,
        RgbTrack colorTrack,
        bool overrideWithFace = false,
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
        bool overrideWithFace = false,
        AnimationFlags flags = AnimationFlags.None
    ) : base(
        (int)duration.TotalMilliseconds,
        flags)
    {
        Tracks = tracks;
        ColorTrack = colorTrack;
        OverrideWithFace = overrideWithFace;
    }

    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, ref AnimationBuffers data)
    {
        var list = new List<Protocol.Track>();
        foreach (Track t in Tracks)
        {
            list.Add(t.ToProtocol(ref data));
        }

        ushort trackOffset = data.StoreTracks(list);
        ushort gradientTrack = data.StoreTrack(ColorTrack.ToProtocol(ref data));
        return new CombinedAnimationData<GradientPatternAnimationData>(shared,
            new GradientPatternAnimationData
            {
                TrackOffset = trackOffset,
                TrackCount = (ushort)Tracks.Count,
                GradientTrackOffset = gradientTrack,
                OverrideWithFace = (byte)(OverrideWithFace ? 1 : 0),
            });
    }
}