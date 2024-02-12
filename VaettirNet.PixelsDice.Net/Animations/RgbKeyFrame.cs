using System;
using System.Drawing;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly struct RgbKeyFrame
{
    public Color Color { get; }
    public uint TimeOffsetMs { get; }

    public RgbKeyFrame(Color color, uint timeOffsetMs)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(timeOffsetMs, (uint)1000);
        Color = color;
        TimeOffsetMs = timeOffsetMs;
    }
    
    public RgbKeyFrame(Color color, TimeSpan timeOffset) : this(color, (uint)timeOffset.TotalMilliseconds)
    {
    }

    internal Protocol.RgbKeyFrame ToProtocol(ref AnimationBuffers data)
    {
        byte palette = data.GetPalette(Color);
        return Protocol.RgbKeyFrame.Create(palette, TimeOffsetMs);
    }
}