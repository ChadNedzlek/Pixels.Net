using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations.Definitions;

public class SimpleAnimation : Animation
{
    public override AnimationType Type => AnimationType.Simple;
    
    public uint FaceMask { get; }
    public Color Color { get; }
    public byte Count { get; }
    public double Fade { get; }

    public SimpleAnimation(
        int durationMs,
        uint faceMask,
        Color color,
        byte count,
        double fade,
        AnimationFlags flags = AnimationFlags.None
    ) : base(durationMs, flags)
    {
        ArgumentValidation.ThrowIfNotUnit(fade);
        FaceMask = faceMask;
        Color = color;
        Count = count;
        Fade = fade;
    }

    public SimpleAnimation(
        TimeSpan duration,
        uint faceMask,
        Color color,
        byte count,
        double fade,
        AnimationFlags flags = AnimationFlags.None
    ) : this((int)duration.TotalMilliseconds, faceMask, color, count, fade, flags)
    {
    }


    private protected override CombinedAnimationData ToProtocol(SharedAnimationData shared, GlobalAnimationData data)
    {
        var color = data.GetPalette(Color);
        return new CombinedAnimationData<SimpleAnimationData>(shared, new SimpleAnimationData
        {
            FaceMask = FaceMask,
            ColorIndex = color,
            Count = Count,
            Fade = (byte)(255 * Fade),
        });
    }
}