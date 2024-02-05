using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly record struct Track(ImmutableList<KeyFrame> Frames, uint LedMask)
{
    internal Protocol.Track ToProtocol(GlobalAnimationData data)
    {
        ushort index = data.StoreKeyFrames(Frames.Select(f => f.ToProtocol(data)));
        return new Protocol.Track { KeyFrameOffset = index, KeyFrameCount = (byte)Frames.Count, LedMask = LedMask };
    }
}

internal static class TrackExtensions
{
    internal static ushort ToProtocol(this IEnumerable<Track> tracks, GlobalAnimationData data)
    {
        return data.StoreTracks(tracks.Select(t => t.ToProtocol(data)));
    }
}