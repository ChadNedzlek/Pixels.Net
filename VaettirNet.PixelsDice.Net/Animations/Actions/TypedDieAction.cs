using System;

namespace VaettirNet.PixelsDice.Net.Animations.Actions;

public abstract class TypedDieAction
{
    public abstract ushort Size { get; }

    public void Write(Span<byte> buffer)
    {
        WriteUniqueData(buffer);
    }

    public void Write(ref Span<byte> buffer)
    {
        WriteUniqueData(buffer);
        buffer = buffer[Size..];
    }

    protected abstract void WriteUniqueData(Span<byte> buffer);
}