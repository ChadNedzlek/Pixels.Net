using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct BleDescriptor
{
    public BleUuid Uuid;
}