using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct BleService
{
    public BleUuid Uuid;

    public nuint DataLength;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
    public byte[] Data = new byte[27];
    
    public nuint CharacteristicCount;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public BleCharacteristic[] Characteristics = new BleCharacteristic[16];

    public BleService()
    {
        Uuid = default;
        DataLength = 0;
        CharacteristicCount = 0;
    }
}