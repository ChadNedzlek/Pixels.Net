using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.AnimationData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SequenceAnimationData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Item
    {
        public ushort Index;
        public ushort Delay;
    }

    public Item Item1;
    public Item Item2;
    public Item Item3;
    public Item Item4;
    public byte ItemCount;
}