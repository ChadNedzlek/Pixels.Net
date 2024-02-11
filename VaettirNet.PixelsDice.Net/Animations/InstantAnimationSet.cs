using System;
using System.Collections.Immutable;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace VaettirNet.PixelsDice.Net.Animations;

public record class InstantAnimationSet(ImmutableList<Animation> Animations)
{
    internal SerializedAnimationData Serialize()
    {
        var data = AnimationBuffers.CreateAnimationOnly();
        foreach (var animation in Animations)
        {
            animation.ToProtocol(ref data);
        }

        (int paddedPaletteSize, _) = SerializationUtils.PadTo4(data.Palette.Count * 3);
        (int animationCountSize, _) = SerializationUtils.PadTo4(data.AnimationBuffer.Count * 2);
        
        if (data.AnimationBuffer.Size >= ushort.MaxValue)
        {
            throw new DeviceOutOfMemoryException("Animation data is too large");
        }

        int bufferSize =
            paddedPaletteSize +
            data.RgbKeyFrames.Count * Unsafe.SizeOf<Protocol.RgbKeyFrame>() +
            data.RgbTracks.Count * Unsafe.SizeOf<Protocol.RgbTrack>() +
            data.KeyFrames.Count * Unsafe.SizeOf<Protocol.KeyFrame>() +
            data.Tracks.Count * Unsafe.SizeOf<Protocol.Track>() +
            animationCountSize +
            data.AnimationBuffer.Size;
        
        var buffer = new byte[bufferSize];
        
        Span<byte> buf = AnimationBuffersToSpan(buffer, data);

        if (!buf.IsEmpty)
        {
            throw new InvalidOperationException("Buffer not consumed");
        }
        return new SerializedAnimationData(data, buffer);
    }

    internal static Span<byte> AnimationBuffersToSpan(byte[] buffer, AnimationBuffers data)
    {
        (_, int palettePadding) = SerializationUtils.PadTo4(data.Palette.Count * 3);
        Span<byte> buf = buffer.AsSpan();
        foreach (Color c in data.Palette)
        {
            SerializationUtils.Write(ref buf, c.R);
            SerializationUtils.Write(ref buf, c.G);
            SerializationUtils.Write(ref buf, c.B);
        }

        for (int i = 0; i < palettePadding; i++)
        {
            SerializationUtils.Write(ref buf, (byte)0);
        }

        SerializationUtils.WriteArray(ref buf, data.RgbKeyFrames);
        SerializationUtils.WriteArray(ref buf, data.RgbTracks);
        SerializationUtils.WriteArray(ref buf, data.KeyFrames);
        SerializationUtils.WriteArray(ref buf, data.Tracks);
        CopyBufferCountAndData(ref buf, ref data.AnimationBuffer);
        return buf;
    }

    internal static void CopyBufferCountAndData(ref Span<byte> destination, ref SpanWriter<byte> buffer)
    {
        (int fullSize, int padding) = SerializationUtils.PadTo4(buffer.Count * 2);
        foreach (int a in buffer.Indices)
        {
            SerializationUtils.Write(ref destination, (ushort)a);
        }
        
        for (int i = 0; i < padding; i++)
        {
            SerializationUtils.Write(ref destination, (byte)0);
        }
        
        buffer.CopyTo(destination);
        destination = destination[buffer.Size..];
    }
}