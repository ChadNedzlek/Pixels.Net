using System;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly struct KeyFrame
{
    public double Intensity { get; }
    public uint Duration { get; }

    public KeyFrame(double intensity, uint duration)
    {
        Intensity = intensity;
        Duration = duration;
    }
    
    public KeyFrame(double intensity, TimeSpan duration) : this(intensity, (uint)duration.TotalMilliseconds)
    {
    }

    internal Protocol.KeyFrame ToProtocol(GlobalAnimationData data)
    {
        return Protocol.KeyFrame.Create((byte)(Intensity * 127), Duration);
    }
}