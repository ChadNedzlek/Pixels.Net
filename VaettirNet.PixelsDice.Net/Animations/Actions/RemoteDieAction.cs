using VaettirNet.PixelsDice.Net.Animations.Protocol.ActionData;

namespace VaettirNet.PixelsDice.Net.Animations.Actions;

public class RemoteDieAction : DieAction
{
    public ushort ActionId { get; }
    
    public RemoteDieAction(ushort actionId)
    {
        ActionId = actionId;
    }

    private protected override TypedDieAction ToTypedAction(ref AnimationBuffers data)
    {
        return new TypedDieAction<RunOnDeviceActionData>(new RunOnDeviceActionData
            { ActionId = ActionId, RemoteActionType = 1 });
    }
}