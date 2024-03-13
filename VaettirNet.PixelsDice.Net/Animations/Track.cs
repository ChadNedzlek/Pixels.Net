using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly record struct Track(ImmutableList<KeyFrame> Frames, uint LedMask)
{
    internal Protocol.Track ToProtocol(ref AnimationBuffers data)
    {
        var keyFrames = new List<Protocol.KeyFrame>();
        foreach (KeyFrame f in Frames)
        {
            keyFrames.Add(f.ToProtocol(ref data));
        }
        ushort index = data.StoreKeyFrames(keyFrames);
        return new Protocol.Track { KeyFrameOffset = index, KeyFrameCount = (byte)Frames.Count, LedMask = LedMask };
    }
    
    public static Track White { get; } = new Track([new KeyFrame(1, 0)], FaceMask.All);

}

internal static class TrackExtensions
{
    internal static ushort ToProtocol(this IEnumerable<Track> tracks, ref AnimationBuffers data)
    {
        var list = new List<Protocol.Track>();
        foreach (Track t in tracks)
        {
            list.Add(t.ToProtocol(ref data));
        }
        return data.StoreTracks(list);
    }
}