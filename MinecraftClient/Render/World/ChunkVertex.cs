using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.World;

public struct ChunkVertex
{
    public Vector2 Uv;
    public Vector4 Color;
    public ushort IndexTexture;
    public byte Brightness;
    public byte SpecialFactors;

    public static byte[] ToBytes(ChunkVertex vertex)
    {
        var size = Marshal.SizeOf(vertex);
        var arr = new byte[size];

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(vertex, ptr, false);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }
}