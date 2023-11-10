using System.Runtime.InteropServices;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Input;

namespace MinecraftLibrary.Engine;

public static class EngineDefaults
{

    public static readonly int ChunkHeight = 16;
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkDepth = 16;

    public static readonly Block[] Blocks = new Block[]
    {
        new AirBlock(), new GrassBlock(), new DirtBlock(), new CobblestoneBlock(), new StoneBlock(), new PlanksBlock(),
        new SaplingBlock()
    };
    public static byte[] GetBytes(ClientInput data)
    {
        var size = Marshal.SizeOf(data);
        var arr = new byte[size];

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, false);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }
    
    public static ClientInput FromBytes(byte[] data, int offset = 0)
    {
        var size = Marshal.SizeOf<ClientInput>();
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(data, offset, ptr, size);
            return Marshal.PtrToStructure<ClientInput>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}