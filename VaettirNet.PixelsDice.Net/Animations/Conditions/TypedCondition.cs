using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

internal abstract class TypedCondition
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

internal class TypedCondition<TConditionData> : TypedCondition where TConditionData : struct
{
    public TConditionData Data;
    
    private static readonly ushort DataSizeConst = (ushort)Unsafe.SizeOf<TConditionData>();
    public override ushort Size => DataSizeConst;

    public TypedCondition(TConditionData data)
    {
        Data = data;
    }
    
    protected override void WriteUniqueData(Span<byte> buffer)
    {
        MemoryMarshal.Write(buffer, Data);
    }
}