using System.Diagnostics;
using System.Runtime.InteropServices;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Engine.States.World;
using MinecraftLibrary.Input;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine;

public static class EngineDefaults
{
    public const int ChunkHeight = 16;
    public const int ChunkWidth = 16;
    public const int ChunkDepth = 16;
    public const float CameraOffset = 1.62f;
    public static readonly Vector3 PlayerSize = new(0.3f, 0.9f, 0.3f);
    public static readonly Vector3 ParticleSize = new(0.1f);

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
    
    public static bool IsIntersectingX(Box3 a, Box3 b)
    {
        return a.Min.X <= b.Max.X && a.Max.X >= b.Min.X;
    }
    
    public static bool IsIntersectingY(Box3 a, Box3 b)
    {
        return a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y;
    }
    
    public static bool IsIntersectingZ(Box3 a, Box3 b)
    {
        return a.Min.Z <= b.Max.Z && a.Max.Z >= b.Min.Z;
    }

    public static bool IsIntersecting(Box3 a, Box3 b)
    {
        return IsIntersectingX(a, b) && IsIntersectingY(a, b) && IsIntersectingZ(a, b);
    }

    public static void ClipCollisionX(Box3 collider, Box3 block, ref float x)
    {
        if (!IsIntersectingY(collider, block) || !IsIntersectingZ(collider, block)) return;

        if (x < 0 && collider.Min.X >= block.Max.X)
            x = Math.Max(x, block.Max.X - collider.Min.X);
        else
            x = Math.Min(x, block.Min.X - collider.Max.X);
    }

    public static void ClipCollisionY(Box3 collider, Box3 block, ref float y)
    {
        if (!IsIntersectingX(collider, block) || !IsIntersectingZ(collider, block)) return;

        if (y < 0 && collider.Min.Y >= block.Max.Y)
            y = Math.Max(y, block.Max.Y - collider.Min.Y);
        else
            y = Math.Min(y, block.Min.Y - collider.Max.Y);
    }

    public static void ClipCollisionZ(Box3 collider, Box3 block, ref float z)
    {
        if (!IsIntersectingX(collider, block) || !IsIntersectingY(collider, block)) return;

        if (z < 0 && collider.Min.Z >= block.Max.Z)
            z = Math.Max(z, block.Max.Z - collider.Min.Z);
        else
            z = Math.Min(z, block.Min.Z - collider.Max.Z);
    }

    public static Vector3 GetFrontVector(float yaw, float pitch)
    {
        var result = new Vector3
        {
            X = (float)(Math.Cos(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch))),
            Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch)),
            Z = (float)(Math.Sin(MathHelper.DegreesToRadians(yaw)) * Math.Cos(MathHelper.DegreesToRadians(pitch)))
        };
        return result;
    }

    public static float GetNextWholeNumberDistance(float x)
    {
        return MathF.Floor(x + 1.0F) - x;
    }

    public static float GetPrevWholeNumberDistance(float x)
    {
        return x - MathF.Ceiling(x - 1.0F);
    }

    public static ushort GetIndexFromVector(Vector3i indexVector)
    {
        return (ushort)(indexVector.X * ChunkHeight * ChunkDepth + indexVector.Y * ChunkDepth + indexVector.Z);
    }

    public static Vector3i GetVectorFromIndex(ushort index)
    {
        return new Vector3i(index / ChunkDepth / ChunkHeight, index / ChunkDepth % ChunkHeight, index % ChunkDepth);
    }

    public delegate void ChunkUpdateHandler(Vector3i chunkPosition, ushort change, BlockType type);

    //public delegate void LightUpdateHandler(Vector2i lightPosition);

    public delegate void EntityUpdateHandler(ushort entityId);

    public delegate void RandomUpdateHandler();

    public delegate void ChunkAddedHandler(ChunkState state);

    public delegate void PlayerAddedHandler(PlayerState state);
}