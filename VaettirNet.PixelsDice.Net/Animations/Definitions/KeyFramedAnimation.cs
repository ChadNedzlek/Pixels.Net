using System;
using System.Collections.Immutable;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class KeyFramedAnimation : Animation
{
    public override AnimationType Type => AnimationType.KeyFramed;
    public ImmutableList<RgbTrack> Tracks { get; }
    
    public KeyFramedAnimation(int durationMs, ImmutableList<RgbTrack> tracks, AnimationFlags flags = AnimationFlags.None) : base(durationMs, flags)
    {
        Tracks = tracks;
    }

    public KeyFramedAnimation(
        TimeSpan duration,
        ImmutableList<RgbTrack> tracks,
        AnimationFlags flags = AnimationFlags.None
    ) : this(
        (int)duration.TotalMilliseconds,
        tracks,
        flags)
    {
    }

    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, AnimationBuffers data)
    {
        ushort offset = Tracks.ToProtocol(data);
        return new CombinedAnimationData<KeyFramedAnimationData>(shared, new KeyFramedAnimationData
        {
            TrackOffset = offset,
            TrackCount = (ushort)Tracks.Count,
        });
    }
}