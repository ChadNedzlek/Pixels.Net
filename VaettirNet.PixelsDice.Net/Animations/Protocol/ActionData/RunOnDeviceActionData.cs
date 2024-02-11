using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Animations.Protocol.ActionData;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RunOnDeviceActionData
{
    public ActionType Type = ActionType.RunOnDevice;
    public byte RemoteActionType;
    public ushort ActionId;

    public RunOnDeviceActionData()
    {
        RemoteActionType = 0;
        ActionId = 0;
    }
}