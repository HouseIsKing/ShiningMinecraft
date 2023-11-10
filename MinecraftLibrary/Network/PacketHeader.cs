using System.Runtime.InteropServices;
using MinecraftLibrary.Input;
using OpenTK.Graphics.ES30;

namespace MinecraftLibrary.Network;

public struct PacketHeader
{
    public PacketType Type;
    public uint Size;
    public static readonly PacketHeader PlayerIdPacket = new PacketHeader(PacketType.PlayerId, sizeof(ushort));

    public static readonly PacketHeader ClientInputHeader =
        new PacketHeader(PacketType.ClientInput, (uint)Marshal.SizeOf<ClientInput>() + sizeof(ulong));

    public PacketHeader(PacketType type)
    {
        Type = type;
    }

    public PacketHeader(PacketType type, uint size) : this(type)
    {
        Size = size;
    }

    public byte[] getBytes() {
        var size = Marshal.SizeOf<PacketHeader>();
        var arr = new byte[size];
        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, false);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }
}