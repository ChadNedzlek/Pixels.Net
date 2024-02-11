using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using VaettirNet.PixelsDice.Net.Animations.Protocol;

namespace VaettirNet.PixelsDice.Net.Animations;

internal ref struct SpanWriter<T>
{
    private Span<T> _buffer;

    public SpanWriter(Span<T> buffer)
    {
        _buffer = buffer;
        Indices = [];
        Size = 0;
    }

    public List<int> Indices { get; }
    public int Size { get; private set; }
    
    public int Count => Indices.Count;
    public int Capacity => _buffer.Length;
    public bool IsValid => !_buffer.IsEmpty;

    public readonly void CopyTo(Span<T> destination)
    {
        _buffer[..Size].CopyTo(destination);
    }

    public int Write(WriteBackCallback handler)
    {
        var i = Size;
        Indices.Add(i);
        int written = handler(_buffer[i..]);
        Size += written;
        return Indices.Count - 1;
    }

    public delegate int WriteBackCallback(Span<T> target);
}

internal ref struct AnimationBuffers
{
    internal readonly List<Color> Palette = [];
    internal readonly List<Protocol.RgbKeyFrame> RgbKeyFrames = [];
    internal readonly List<Protocol.RgbTrack> RgbTracks = [];
    internal readonly List<Protocol.KeyFrame> KeyFrames = [];
    internal readonly List<Protocol.Track> Tracks = [];
    internal SpanWriter<byte> AnimationBuffer;
    internal SpanWriter<byte> ConditionBuffer;
    internal SpanWriter<byte> ActionBuffer;
    internal readonly List<Protocol.AnimationRuleData> Rules = [];

    public AnimationBuffers()
    {
        AnimationBuffer = default;
        ConditionBuffer = default;
        ActionBuffer = default;
    }

    public AnimationBuffers(SpanWriter<byte> animations, SpanWriter<byte> conditions, SpanWriter<byte> actions)
    {
        AnimationBuffer = animations;
        ConditionBuffer = conditions;
        ActionBuffer = actions;
    }

    public static AnimationBuffers CreateHeap()
    {
        return new AnimationBuffers(new SpanWriter<byte>(new byte[6400]),
            new SpanWriter<byte>(new byte[6400]),
            new SpanWriter<byte>(new byte[6400])
            );
    }

    public static AnimationBuffers CreateAnimationOnly()
    {
        return new AnimationBuffers(new SpanWriter<byte>(new byte[6400]), default, default);
    }

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

    public ushort StoreRule(AnimationRuleData rule)
    {
        var ret = Rules.Count;
        Rules.Add(rule);
        return AsIndex(ret);
    }
}