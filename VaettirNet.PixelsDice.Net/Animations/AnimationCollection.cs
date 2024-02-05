using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations;

public record class AnimationCollection(ImmutableList<Animation> Animations)
{
    internal SerializedAnimationData Serialize()
    {
        var data = new GlobalAnimationData();
        var animList = Animations.Select(a => a.ToProtocol(data)).ToList();
        int realPaletteSize = data.Palette.Count * 3;
        int alignedBufferSize = RoundTo4(realPaletteSize);
        int palettePadding = alignedBufferSize - realPaletteSize;
        int realAnimationCountSize = Animations.Count * 2;
        int alignedAnimationCountSize = RoundTo4(realAnimationCountSize);
        int animationCountPadding = alignedAnimationCountSize - realAnimationCountSize;

        int animationSize = animList.Sum(a => a.Size);
        if (animationSize >= ushort.MaxValue)
        {
            throw new DeviceOutOfMemoryException("Animation data is too large");
        }

        int bufferSize =
            alignedBufferSize +
            data.RgbKeyFrames.Count * Unsafe.SizeOf<Protocol.RgbKeyFrame>() +
            data.RgbTracks.Count * Unsafe.SizeOf<Protocol.RgbTrack>() +
            data.KeyFrames.Count * Unsafe.SizeOf<Protocol.KeyFrame>() +
            data.Tracks.Count * Unsafe.SizeOf<Protocol.Track>() +
            alignedAnimationCountSize +
            animationSize;
        byte[] buffer = new byte[bufferSize];
        var buf = buffer.AsSpan();
        foreach (var c in data.Palette)
        {
            Write(ref buf, c.R);
            Write(ref buf, c.G);
            Write(ref buf, c.B);
        }

        for (int i = 0; i < palettePadding; i++)
        {
            Write(ref buf, (byte)0);
        }

        WriteArray(ref buf, data.RgbKeyFrames);
        WriteArray(ref buf, data.RgbTracks);
        WriteArray(ref buf, data.KeyFrames);
        WriteArray(ref buf, data.Tracks);

        ushort animOffset = 0;
        foreach (CombinedAnimationData a in animList)
        {
            Write(ref buf, animOffset);
            animOffset += a.Size;
        }
        
        for (int i = 0; i < animationCountPadding; i++)
        {
            Write(ref buf, (byte)0);
        }
        
        foreach (CombinedAnimationData a in animList)
        {
            a.Write(ref buf);
        }

        if (!buf.IsEmpty)
        {
            throw new InvalidOperationException("Buffer not consumed");
        }
        return new SerializedAnimationData((ushort)animationSize, data, buffer);
    }

    private void WriteArray<T>(ref Span<byte> buffer, IEnumerable<T> arr) where T : struct
    {
        foreach (var item in arr)
        {
            Write(ref buffer, item);
        }
    }

    private void Write<T>(ref Span<byte> buffer, T thing) where T : struct
    {
        MemoryMarshal.Write(buffer, thing);
        buffer = buffer[Unsafe.SizeOf<T>()..];
    }

    private int RoundTo4(int value)
    {
        return (value + 3) / 4 * 4;
    }
}