using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential)]
internal struct RollStateMessage
{
    public byte Id;
    public RollState RollState;
    public byte CurrentFace;
}