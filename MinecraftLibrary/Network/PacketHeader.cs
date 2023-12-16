using System.Runtime.InteropServices;
using MinecraftLibrary.Input;
using OpenTK.Graphics.ES30;

namespace MinecraftLibrary.Network;

public struct PacketHeader(PacketType type)
{
    public PacketType Type = type;
    public uint Size;
    public static readonly PacketHeader PlayerIdPacket = new PacketHeader(PacketType.PlayerId, sizeof(ushort));

    public static readonly PacketHeader ClientInputHeader =
        new(PacketType.ClientInput, (uint)Marshal.SizeOf<ClientInput>() + sizeof(ulong));

    private PacketHeader(PacketType type, uint size) : this(type)
    {
        Size = size;
    }

    public readonly byte[] GetBytes()
    {
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