// Many padding fields for struct alignment 
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ActionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PlayAnimationActionData
{
    public ActionType Type = ActionType.PlayAnimation;
    public byte AnimationIndex;
    public byte FaceIndex;
    public byte LoopCount;

    public PlayAnimationActionData()
    {
        AnimationIndex = 0;
        FaceIndex = 0;
        LoopCount = 0;
    }
}