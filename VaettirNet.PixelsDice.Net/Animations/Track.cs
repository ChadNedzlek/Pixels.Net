using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly record struct Track(ImmutableList<KeyFrame> Frames, uint LedMask)
{
    internal Protocol.Track ToProtocol(AnimationBuffers data)
    {
        var keyFrames = new List<Protocol.KeyFrame>();
        foreach (KeyFrame f in Frames)
        {
            keyFrames.Add(f.ToProtocol(data));
        }
        ushort index = data.StoreKeyFrames(keyFrames);
        return new Protocol.Track { KeyFrameOffset = index, KeyFrameCount = (byte)Frames.Count, LedMask = LedMask };
    }
}

internal static class TrackExtensions
{
    internal static ushort ToProtocol(this IEnumerable<Track> tracks, AnimationBuffers data)
    {
        var list = new List<Protocol.Track>();
        foreach (Track t in tracks)
        {
            list.Add(t.ToProtocol(data));
        }
        return data.StoreTracks(list);
    }
}