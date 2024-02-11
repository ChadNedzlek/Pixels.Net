using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly record struct RgbTrack(ImmutableList<RgbKeyFrame> Frames, uint LedMask)
{
    internal Protocol.RgbTrack ToProtocol(AnimationBuffers data)
    {
        var list = new List<Protocol.RgbKeyFrame>();
        foreach (RgbKeyFrame f in Frames)
        {
            list.Add(f.ToProtocol(data));
        }

        ushort index = data.StoreKeyFrames(list);
        return new Protocol.RgbTrack { KeyFrameOffset = index, KeyFrameCount = (byte)Frames.Count, LedMask = LedMask };
    }
}

internal static class RgbTrackExtensions
{
    internal static ushort ToProtocol(this IEnumerable<RgbTrack> tracks, AnimationBuffers data)
    {
        var list = new List<Protocol.RgbTrack>();
        foreach (RgbTrack t in tracks)
        {
            list.Add(t.ToProtocol(data));
        }

        return data.StoreTracks(list);
    }
}