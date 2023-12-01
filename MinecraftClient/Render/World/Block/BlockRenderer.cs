using MinecraftClient.Render.Textures;
using MinecraftLibrary.Engine.Blocks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Buffer = System.Buffer;

namespace MinecraftClient.Render.World.Block;

public abstract class BlockRenderer(MinecraftLibrary.Engine.Blocks.Block block)
{
    private static readonly BlockRenderer[] BlockRenderers = { new AirBlockRenderer(), new GrassBlockRenderer(), new DirtBlockRenderer(), new CobblestoneBlockRenderer(), new StoneBlockRenderer() };
    
    protected List<Texture> Textures { get; } = new();
    private readonly MinecraftLibrary.Engine.Blocks.Block _block = block;
    private static int _ssbo;
    private static readonly byte[] SsboData;
    private const float XSideShade = 0.6F;
    private const float YSideShade = 1.0F;
    private const float ZSideShade = 0.8F;

    static BlockRenderer()
    {
        SsboData = new byte[(2 * 16 + 6 * 16) * BlockRenderers.Length];
        var offset = 0;
        foreach (var blockRender in BlockRenderers)
        {
            var box = blockRender._block.BlockBounds;
            var vertices = new[]
            {
                box.Min.X, box.Min.Y, box.Min.Z, 1.0f,
                box.Max.X, box.Max.Y, box.Max.Z, 1.0f
            };
            Buffer.BlockCopy(vertices, 0, SsboData, offset, 2 * 16);
            offset += 2 * 16;
            var textureIndexes = new uint[]
            {
                blockRender.GetIndexTexture(BlockFaces.Top), blockRender.GetColor(BlockFaces.Top), blockRender.GetSpecialFactor(), 0,
                blockRender.GetIndexTexture(BlockFaces.Bottom), blockRender.GetColor(BlockFaces.Bottom), blockRender.GetSpecialFactor(), 0,
                blockRender.GetIndexTexture(BlockFaces.East), blockRender.GetColor(BlockFaces.East), blockRender.GetSpecialFactor(), 0,
                blockRender.GetIndexTexture(BlockFaces.West), blockRender.GetColor(BlockFaces.West), blockRender.GetSpecialFactor(), 0,
                blockRender.GetIndexTexture(BlockFaces.North), blockRender.GetColor(BlockFaces.North), blockRender.GetSpecialFactor(), 0,
                blockRender.GetIndexTexture(BlockFaces.South), blockRender.GetColor(BlockFaces.South), blockRender.GetSpecialFactor(), 0,
            };
            Buffer.BlockCopy(textureIndexes, 0, SsboData, offset, 16 * 6);
            offset += 16 * 6;
        }
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

    protected virtual uint GetSpecialFactor()
    {
        return 0;
    }
}