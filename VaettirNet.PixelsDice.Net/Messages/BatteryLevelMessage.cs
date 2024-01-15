using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential)]
internal struct BatteryLevelMessage
{
    public byte Id;
    public byte BatteryLevel;
    public byte BatteryState;
}