namespace VaettirNet.PixelsDice.Net.Animations;

internal ref struct SerializedAnimationData
{
    public SerializedAnimationData(AnimationBuffers data, byte[] buffer)
    {
        Data = data;
        Buffer = buffer;
    }

    public readonly AnimationBuffers Data;
    public readonly byte[] Buffer;
}