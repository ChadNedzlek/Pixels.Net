using System.Runtime.InteropServices;

namespace Pixels.Net.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct BleCharacteristic
{
    public BleUuid Uuid;

    [MarshalAs(UnmanagedType.U1)] public bool CanRead;
    [MarshalAs(UnmanagedType.U1)] public bool CanReadRequest;
    [MarshalAs(UnmanagedType.U1)] public bool CanWriteCommand;
    [MarshalAs(UnmanagedType.U1)] public bool CanNotify;
    [MarshalAs(UnmanagedType.U1)] public bool CanIndicate;

    public nuint DescriptorCount;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public BleDescriptor[] Descriptors = new BleDescriptor[16];

    public BleCharacteristic()
    {
        Uuid = default;
        CanRead = false;
        CanReadRequest = false;
        CanWriteCommand = false;
        CanNotify = false;
        CanIndicate = false;
        DescriptorCount = 0;
    }
}