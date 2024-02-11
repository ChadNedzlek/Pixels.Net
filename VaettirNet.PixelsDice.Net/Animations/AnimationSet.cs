using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using VaettirNet.PixelsDice.Net.Animations.Protocol;

namespace VaettirNet.PixelsDice.Net.Animations;

public record class AnimationSet(ImmutableList<AnimationRule> Rules)
{
    internal SerializedAnimationData Serialize()
    {
        var data = AnimationBuffers.CreateHeap();
        foreach (var rule in Rules)
        {
            rule.ToProtocol(ref data);
        }
        
        (int paddedPaletteSize, _) = SerializationUtils.PadTo4(data.Palette.Count * 3);
        (int animationCountSize, _) = SerializationUtils.PadTo4(data.AnimationBuffer.Count * 2);
        (int conditionCountSize, _) = SerializationUtils.PadTo4(data.ConditionBuffer.Count * 2);
        (int actionCountSize, _) = SerializationUtils.PadTo4(data.ConditionBuffer.Count * 2);
        int bufferSize =
            paddedPaletteSize +
            data.RgbKeyFrames.Count * Unsafe.SizeOf<Protocol.RgbKeyFrame>() +
            data.RgbTracks.Count * Unsafe.SizeOf<Protocol.RgbTrack>() +
            data.KeyFrames.Count * Unsafe.SizeOf<Protocol.KeyFrame>() +
            data.Tracks.Count * Unsafe.SizeOf<Protocol.Track>() +
            animationCountSize +
            data.AnimationBuffer.Size +
            conditionCountSize +
            data.ConditionBuffer.Size +
            actionCountSize +
            data.ActionBuffer.Size +
            Rules.Count * Unsafe.SizeOf<AnimationRuleData>()
            + 4 /* "Behavior" class */;

        var buffer = new byte[bufferSize];

        Span<byte> buf = InstantAnimationSet.AnimationBuffersToSpan(buffer, data);
        InstantAnimationSet.CopyBufferCountAndData(ref buf, ref data.ConditionBuffer);
        InstantAnimationSet.CopyBufferCountAndData(ref buf, ref data.ActionBuffer);
        SerializationUtils.WriteArray(ref buf, data.Rules);
        SerializationUtils.Write(ref buf, (ushort) 0);
        SerializationUtils.Write(ref buf, (ushort) data.Rules.Count);
        
        if (!buf.IsEmpty)
        {
            throw new InvalidOperationException("Buffer not consumed");
        }

        return new SerializedAnimationData(data, buffer);
    }
}