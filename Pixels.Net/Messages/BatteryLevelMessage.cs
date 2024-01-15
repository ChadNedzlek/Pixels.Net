using System.Runtime.InteropServices;

namespace Pixels.Net.Messages;

[StructLayout(LayoutKind.Sequential)]
internal struct BatteryLevelMessage
{
    public byte Id;
    public byte BatteryLevel;
    public byte BatteryState;
}