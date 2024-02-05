using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class BlinkIdAnimation : Animation
{
    public override AnimationType Type => AnimationType.BlinkId;
    public byte FramesPerBlink { get; }
    public double Brightness { get; }

    public BlinkIdAnimation(
        int durationMs,
        byte framesPerBlink,
        double brightness,
        AnimationFlags flags = AnimationFlags.None) : base(durationMs, flags)
    {
        ArgumentValidation.ThrowIfNotUnit(brightness);
        FramesPerBlink = framesPerBlink;
        Brightness = brightness;
    }

    public BlinkIdAnimation(
        TimeSpan duration,
        byte framesPerBlink,
        double brightness,
        AnimationFlags flags = AnimationFlags.None) : this((int)duration.TotalMilliseconds, framesPerBlink, brightness, flags)
    {
    }

    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, GlobalAnimationData data)
    {
        return new CombinedAnimationData<BlinkIdAnimationData>(shared,
            new BlinkIdAnimationData
            {
                FramesPerBlink = FramesPerBlink,
                Brightness = (byte)(Brightness * 255)
            });
    }
}