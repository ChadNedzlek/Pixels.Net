using System.Runtime.InteropServices;

namespace Pixels.Net.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct BleUuid
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 37)]
    public string Value;

    public BleUuid(string value)
    {
        Value = value;
    }
}