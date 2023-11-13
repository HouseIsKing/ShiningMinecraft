using System.Diagnostics;
using System.Runtime.InteropServices;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Input;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine;

public static class EngineDefaults
{
    public const int ChunkHeight = 16;
    public const int ChunkWidth = 16;
    public const int ChunkDepth = 16;

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
    
    public static long NanoTime() {
        var nano = 10000L * Stopwatch.GetTimestamp();
        nano /= TimeSpan.TicksPerMillisecond;
        nano *= 100L;
        return nano;
    }
    
    public static Box3 Expand(Box3 box, Vector3 vector)
    {
        var min = box.Min;
        var max = box.Max;
        if (vector.X < 0)
            min.X += vector.X;
        else
            max.X += vector.X;
        if (vector.Y < 0)
            min.Y += vector.Y;
        else
            max.Y += vector.Y;
        if (vector.Z < 0)
            min.Z += vector.Z;
        else
            max.Z += vector.Z;
        return new Box3(min, max);
    }
    public delegate void ChunkUpdateHandler(Vector3i chunkPosition);

    public delegate void LightUpdateHandler(Vector2i lightPosition);

    public delegate void EntityUpdateHandler(ushort entityId);
}