using System.Runtime.InteropServices;

namespace Pixels.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct IAmADieMessage
{
    public byte Id;
    public byte LedCount;
    public Colorway Colorway;
    public DieType Type;
    private int DataSetHash;
    public uint PixelId;
    public ushort AvailableFlash;
    public int BuildTimestamp;
    public RollState RollState;
    public byte CurrentFace;
    public byte BatteryLevel;
    public byte BatteryState;
}