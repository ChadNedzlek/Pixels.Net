using System.Runtime.InteropServices;

namespace Pixels.Net.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct BleDescriptor
{
    public BleUuid Uuid;
}