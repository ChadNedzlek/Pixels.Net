using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct BleManufacturerData
{
    public ushort ManufacturerId;
    public nuint DataLength;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
    public byte[] Data;
}