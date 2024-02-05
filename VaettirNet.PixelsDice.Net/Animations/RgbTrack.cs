using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly record struct RgbTrack(ImmutableList<RgbKeyFrame> Frames, uint LedMask)
{
    internal Protocol.RgbTrack ToProtocol(GlobalAnimationData data)
    {
        ushort index = data.StoreKeyFrames(Frames.Select(f => f.ToProtocol(data)));
        return new Protocol.RgbTrack { KeyFrameOffset = index, KeyFrameCount = (byte)Frames.Count, LedMask = LedMask };
    }
}

internal static class RgbTrackExtensions
{
    internal static ushort ToProtocol(this IEnumerable<RgbTrack> tracks, GlobalAnimationData data)
    {
        return data.StoreTracks(tracks.Select(t => t.ToProtocol(data)));
    }
}