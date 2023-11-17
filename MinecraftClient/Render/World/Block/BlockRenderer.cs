using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.World.Block;

public abstract class BlockRenderer
{
    private const float XSideShade = 0.6F;
    private const float YSideShade = 1.0F;
    private const float ZSideShade = 0.8F;

    public virtual ChunkVertex GenerateFaceVertex(BlockFaces face, byte triangleIndex,
        Vector4 color, byte brightness)
    {
        return new ChunkVertex
        {
            Uv = GetUVFromTriangleIndex(triangleIndex), Brightness = brightness, Color = color,
            IndexTexture = GetIndexTexture(face), SpecialFactors = 0
        };
    }

    protected virtual Vector2 GetUVFromTriangleIndex(byte triangleIndex)
    {
        return triangleIndex switch
        {
            0 => new Vector2(),
            1 => new Vector2(1, 0),
            2 => new Vector2(0, 1),
            3 => new Vector2(1, 1),
            _ => throw new Exception("Invalid triangle index")
        };
    }

    protected abstract ushort GetIndexTexture(BlockFaces face);

}