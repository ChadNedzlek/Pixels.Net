using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations;

public abstract class Animation
{
    public abstract AnimationType Type { get; }
    public AnimationFlags Flags { get; }
    public int DurationMs { get; }

    protected Animation(int durationMs, AnimationFlags flags = AnimationFlags.None)
    {
        ArgumentValidation.ThrowIfOutOfRange(durationMs, 0, ushort.MaxValue);
        Flags = flags;
        DurationMs = durationMs;
    }

    internal int ToProtocol(ref AnimationBuffers data)
    {
        ref SpanWriter<byte> buffer = ref data.AnimationBuffer;
        CombinedAnimationData protocol = ToProtocol(
            new SharedAnimationData
            {
                Type = Type,
                Flags = Flags,
                Duration = (ushort)DurationMs,
            },
            ref data);
        return buffer.Write(b =>
        {
            protocol.Write(b);
            return protocol.Size;
        });
    }

    private protected abstract CombinedAnimationData ToProtocol(SharedAnimationData shared, ref AnimationBuffers data);
}