using System.Runtime.InteropServices;

namespace Pixels.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct WhoAmIMessage
{
    public byte Id = 1;

    public WhoAmIMessage()
    {
    }
}