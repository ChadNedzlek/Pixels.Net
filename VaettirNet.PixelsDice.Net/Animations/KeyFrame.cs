using System;

namespace VaettirNet.PixelsDice.Net.Animations;

public readonly struct KeyFrame
{
    public double Intensity { get; }
    public uint TimeOffsetMs { get; }

    public KeyFrame(double intensity, uint timeOffsetMs)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(timeOffsetMs, (uint)1000);
        ArgumentValidation.ThrowIfNotUnit(intensity);
        Intensity = intensity;
        TimeOffsetMs = timeOffsetMs;
    }
    
    public KeyFrame(double intensity, TimeSpan timeOffset) : this(intensity, (uint)timeOffset.TotalMilliseconds)
    {
    }

    internal Protocol.KeyFrame ToProtocol(ref AnimationBuffers data)
    {
        return Protocol.KeyFrame.Create((byte)(Intensity * 127), TimeOffsetMs);
    }
}