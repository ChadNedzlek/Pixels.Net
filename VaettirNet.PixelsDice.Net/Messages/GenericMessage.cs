using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace VaettirNet.PixelsDice.Net.Messages;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct GenericMessage
{
    public MessageType Id;
    
    public GenericMessage(MessageType id)
    {
        Id = id;
    }
}