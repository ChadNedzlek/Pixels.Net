using System;
using System.Drawing;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly struct RgbKeyFrame
{
    public Color Color { get; }
    public uint Duration { get; }

    public RgbKeyFrame(Color color, uint duration)
    {
        Color = color;
        Duration = duration;
    }
    
    public RgbKeyFrame(Color color, TimeSpan duration) : this(color, (uint)duration.TotalMilliseconds)
    {
    }

    internal Protocol.RgbKeyFrame ToProtocol(GlobalAnimationData data)
    {
        byte palette = data.GetPalette(Color);
        return Protocol.RgbKeyFrame.Create(palette, Duration);
    }
}