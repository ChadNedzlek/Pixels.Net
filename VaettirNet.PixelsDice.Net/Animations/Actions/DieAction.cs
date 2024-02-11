using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VaettirNet.PixelsDice.Net.Animations.Conditions;

namespace VaettirNet.PixelsDice.Net.Animations.Actions;

public abstract class DieAction
{
    internal ushort ToProtocol(ref AnimationBuffers data)
    {
        TypedDieAction typed = ToTypedAction(ref data);
        ref SpanWriter<byte> buffer = ref data.ActionBuffer;
        return (ushort)buffer.Write(b =>
        {
            typed.Write(b);
            return typed.Size;
        });
    }

    private protected abstract TypedDieAction ToTypedAction(ref AnimationBuffers data);
}

public class TypedDieAction<TAction> : TypedDieAction where TAction : struct
{
    public TAction Data;
    
    private static readonly ushort DataSizeConst = (ushort)Unsafe.SizeOf<TAction>();
    public override ushort Size => DataSizeConst;

    public TypedDieAction(TAction data)
    {
        Data = data;
    }
    
    protected override void WriteUniqueData(Span<byte> buffer)
    {
        MemoryMarshal.Write(buffer, Data);
    }
}