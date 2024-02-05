using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

namespace VaettirNet.PixelsDice.Net.Animations;

internal abstract class CombinedAnimationData
{
    protected static readonly ushort SharedDataSize = (ushort)Unsafe.SizeOf<SharedAnimationData>();
    public SharedAnimationData Shared;
    
    public ushort Size => (ushort)(SharedDataSize + DataSize);
    protected abstract ushort DataSize { get; }

    protected CombinedAnimationData(SharedAnimationData shared)
    {
        Shared = shared;
    }

    public void Write(Span<byte> buffer)
    {
        MemoryMarshal.Write(buffer, Shared);
        WriteUniqueData(buffer);
    }

    public void Write(ref Span<byte> buffer)
    {
        MemoryMarshal.Write(buffer, Shared);
        buffer = buffer[SharedDataSize..];
        WriteUniqueData(buffer);
        buffer = buffer[DataSize..];
    }

    protected abstract void WriteUniqueData(Span<byte> buffer);
}

internal class CombinedAnimationData<TAnimationData> : CombinedAnimationData where TAnimationData : struct
{
    public TAnimationData Data;
    
    private static readonly ushort DataSizeConst = (ushort)Unsafe.SizeOf<TAnimationData>();
    protected override ushort DataSize => DataSizeConst;

    public CombinedAnimationData(SharedAnimationData shared, TAnimationData data) : base(shared)
    {
        Data = data;
    }
    
    protected override void WriteUniqueData(Span<byte> buffer)
    {
        MemoryMarshal.Write(buffer, Data);
    }
}