using System;
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

    internal CombinedAnimationData ToProtocol(GlobalAnimationData data)
    {
        return ToProtocol(
            new SharedAnimationData
            {
                Duration = (ushort)DurationMs,
                Type = Type,
                Flags = Flags
            },
            data);
    }

    private protected abstract CombinedAnimationData ToProtocol(SharedAnimationData shared, GlobalAnimationData data);
}