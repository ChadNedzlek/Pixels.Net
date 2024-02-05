using System;

namespace VaettirNet.PixelsDice.Net.Animations;

public static class AnimationUtils
{
    public static uint Hash(Span<byte> data)
    {
        // Copied from https://github.com/GameWithPixels/DiceFirmware/blob/main/src/utils/Utils.cpp
        uint hash = 5381;
        foreach (byte b in data)
        {
            hash = 33 * hash ^ b;
        }
        return hash;
    }
}