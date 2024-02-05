using System;

namespace VaettirNet.PixelsDice.Net.Animations;

public class DeviceOutOfMemoryException : Exception
{
    public DeviceOutOfMemoryException(string message) : base(message)
    {
    }
}