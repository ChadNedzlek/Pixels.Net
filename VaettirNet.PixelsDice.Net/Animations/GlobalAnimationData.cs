using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace VaettirNet.PixelsDice.Net.Animations;

internal class GlobalAnimationData
{
    internal readonly List<Color> Palette = [];
    internal readonly List<Protocol.RgbKeyFrame> RgbKeyFrames = [];
    internal readonly List<Protocol.RgbTrack> RgbTracks = [];
    internal readonly List<Protocol.KeyFrame> KeyFrames = [];
    internal readonly List<Protocol.Track> Tracks = [];
    
    public byte GetPalette(Color color)
    {
        var rgb = Color.FromArgb(0, color);
        var i = Palette.IndexOf(rgb);
        if (i == -1)
        {
            if (Palette.Count >= 128)
            {
                throw new DeviceOutOfMemoryException("Color cannot be added to palette, only 128 colors supported");
            }

            i = Palette.Count;
            Palette.Add(rgb);
        }
        return (byte)i;
    }

    private ushort AsIndex(int index, [CallerMemberName] string callerMember = null)
    {
        if (index > ushort.MaxValue)
        {
            throw new DeviceOutOfMemoryException($"Cannot {callerMember}, only {ushort.MaxValue} allowed");
        }

        return (ushort)index;
    }

    public ushort StoreKeyFrame(Protocol.RgbKeyFrame keyFrame)
    {
        var ret = RgbKeyFrames.Count;
        RgbKeyFrames.Add(keyFrame);
        return AsIndex(ret);
    }
    
    public ushort StoreKeyFrames(IEnumerable<Protocol.RgbKeyFrame> keyFrames)
    {
        var ret = RgbKeyFrames.Count;
        RgbKeyFrames.AddRange(keyFrames);
        return AsIndex(ret);
    }

    public ushort StoreTrack(Protocol.RgbTrack track)
    {
        var ret = RgbTracks.Count;
        RgbTracks.Add(track);
        return AsIndex(ret);
    }

    public ushort StoreTracks(IEnumerable<Protocol.RgbTrack> track)
    {
        var ret = RgbTracks.Count;
        RgbTracks.AddRange(track);
        return AsIndex(ret);
    }

    public ushort StoreKeyFrame(Protocol.KeyFrame keyFrame)
    {
        var ret = KeyFrames.Count;
        KeyFrames.Add(keyFrame);
        return AsIndex(ret);
    }
    
    public ushort StoreKeyFrames(IEnumerable<Protocol.KeyFrame> keyFrames)
    {
        var ret = KeyFrames.Count;
        KeyFrames.AddRange(keyFrames);
        return AsIndex(ret);
    }

    public ushort StoreTrack(Protocol.Track track)
    {
        var ret = Tracks.Count;
        Tracks.Add(track);
        return AsIndex(ret);
    }
    
    public ushort StoreTracks(IEnumerable<Protocol.Track> track)
    {
        var ret = Tracks.Count;
        Tracks.AddRange(track);
        return AsIndex(ret);
    }
}