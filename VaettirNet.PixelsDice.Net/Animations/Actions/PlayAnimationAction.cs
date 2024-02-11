using VaettirNet.PixelsDice.Net.Animations.Protocol.ActionData;

namespace VaettirNet.PixelsDice.Net.Animations.Actions;

public class PlayAnimationAction : DieAction
{
    public Animation Animation { get; }
    public byte FaceIndex { get; }
    public byte LoopCount { get; }
    
    public PlayAnimationAction(byte loopCount, byte faceIndex, Animation animation)
    {
        LoopCount = loopCount;
        FaceIndex = faceIndex;
        Animation = animation;
    }

    private protected override TypedDieAction ToTypedAction(ref AnimationBuffers data)
    {
        int index = Animation.ToProtocol(ref data);
        if (index > byte.MaxValue)
        {
            throw new DeviceOutOfMemoryException("Only 255 animations supported in actions");
        }

        return new TypedDieAction<PlayAnimationActionData>(new PlayAnimationActionData
        {
            AnimationIndex = (byte)index,
            FaceIndex = FaceIndex,
            LoopCount = LoopCount,
        });
    }
}