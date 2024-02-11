namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public abstract class Condition
{
    internal ushort ToProtocol(ref AnimationBuffers data)
    {
        ref SpanWriter<byte> buffer = ref data.ConditionBuffer;
        TypedCondition typed = ToProtocol();
        return (ushort)buffer.Write(b =>
        {
            typed.Write(b);
            return typed.Size;
        });
    }

    private protected abstract TypedCondition ToProtocol();
}