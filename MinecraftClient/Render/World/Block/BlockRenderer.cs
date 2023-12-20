using MinecraftClient.Render.Textures;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Buffer = System.Buffer;

namespace MinecraftClient.Render.World.Block;

public abstract class BlockRenderer(MinecraftLibrary.Engine.Blocks.Block block)
{
    private static readonly BlockRenderer[] BlockRenderers = new BlockRenderer[Enum.GetValues<BlockType>().Length];
    
    protected List<Texture> Textures { get; } = [];
    private readonly MinecraftLibrary.Engine.Blocks.Block _block = block;
    private static int _ssbo;
    private static readonly byte[] SsboData;
    private const float XSideShade = 0.6F;
    private const float YSideShade = 1.0F;
    private const float ZSideShade = 0.8F;
    public DrawType DrawType { get; protected set; } = DrawType.Normal;

    static BlockRenderer()
    {
        GenerateBlockRenderers();
        SsboData = new byte[(2 * 16 + 6 * 16) * BlockRenderers.Length];
        var offset = 0;
        foreach (var blockRender in BlockRenderers)
        {
            var box = blockRender._block.BlockBounds;
            var vertices = new[]
            {
                box.Min.X, box.Min.Y, box.Min.Z, 0.0F,
                box.Max.X, box.Max.Y, box.Max.Z, 0.0F,
            };
            Buffer.BlockCopy(vertices, 0, SsboData, offset, 2 * 16);
            offset += 2 * 16;
            var textureIndexes = new uint[]
            {
                blockRender.GetIndexTexture(BlockFaces.Top), blockRender.GetColor(BlockFaces.Top), 0, 0,
                blockRender.GetIndexTexture(BlockFaces.Bottom), blockRender.GetColor(BlockFaces.Bottom), 0, 0,
                blockRender.GetIndexTexture(BlockFaces.East), blockRender.GetColor(BlockFaces.East), 0, 0,
                blockRender.GetIndexTexture(BlockFaces.West), blockRender.GetColor(BlockFaces.West), 0, 0,
                blockRender.GetIndexTexture(BlockFaces.North), blockRender.GetColor(BlockFaces.North), 0, 0,
                blockRender.GetIndexTexture(BlockFaces.South), blockRender.GetColor(BlockFaces.South), 0, 0
            };
            Buffer.BlockCopy(textureIndexes, 0, SsboData, offset, 16 * 6);
            offset += 16 * 6;
        }
    }

    private static void GenerateBlockRenderers()
    {
        foreach (var type in Enum.GetValues<BlockType>())
            BlockRenderers[(int)type] = type switch
            {
                BlockType.Cobblestone => new CobblestoneBlockRenderer(),
                BlockType.Dirt => new DirtBlockRenderer(),
                BlockType.Grass => new GrassBlockRenderer(),
                BlockType.Stone => new StoneBlockRenderer(),
                BlockType.Planks => new PlanksBlockRenderer(),
                BlockType.Sapling => new SaplingBlockRenderer(),
                _ => new AirBlockRenderer()
            };
    }

    protected abstract uint GetIndexTexture(BlockFaces face);

    protected static (byte, byte, byte, byte) GetRgba(BlockFaces face)
    {
        byte r = byte.MaxValue, g = byte.MaxValue, b = byte.MaxValue;
        const byte a = byte.MaxValue;
        switch (face)
        {
            case BlockFaces.Top:
                r = (byte)(byte.MaxValue * YSideShade);
                g = (byte)(byte.MaxValue * YSideShade);
                b = (byte)(byte.MaxValue * YSideShade);
                break;
            case BlockFaces.Bottom:
                r = (byte)(byte.MaxValue * YSideShade);
                g = (byte)(byte.MaxValue * YSideShade);
                b = (byte)(byte.MaxValue * YSideShade);
                break;
            case BlockFaces.East:
                r = (byte)(byte.MaxValue * XSideShade);
                g = (byte)(byte.MaxValue * XSideShade);
                b = (byte)(byte.MaxValue * XSideShade);
                break;
            case BlockFaces.West:
                r = (byte)(byte.MaxValue * XSideShade);
                g = (byte)(byte.MaxValue * XSideShade);
                b = (byte)(byte.MaxValue * XSideShade);
                break;
            case BlockFaces.North:
                r = (byte)(byte.MaxValue * ZSideShade);
                g = (byte)(byte.MaxValue * ZSideShade);
                b = (byte)(byte.MaxValue * ZSideShade);
                break;
            case BlockFaces.South:
                r = (byte)(byte.MaxValue * ZSideShade);
                g = (byte)(byte.MaxValue * ZSideShade);
                b = (byte)(byte.MaxValue * ZSideShade);
                break;
        }
        return (r, g, b, a);
    }

    public static void Terminate()
    {
        GL.DeleteBuffer(_ssbo);
    }

    public static void Setup()
    { 
        GL.CreateBuffers(1, out _ssbo);
        GL.NamedBufferStorage(_ssbo, SsboData.Length, SsboData, BufferStorageFlags.None);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, _ssbo);
    }

    protected virtual uint GetColor(BlockFaces face)
    {
        var (r, g, b, a) = GetRgba(face);
        return (uint)((r << 24) | (g << 16) | (b << 8) | a);
    }

    public static BlockRenderer GetBlockRenderer(BlockType type)
    {
        return BlockRenderers[(int)type];
    }
}