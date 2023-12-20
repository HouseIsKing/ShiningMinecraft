using MinecraftClient.Render.Textures;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;

namespace MinecraftClient.Render.World.Block;

internal sealed class SaplingBlockRenderer : BlockRenderer
{
    public SaplingBlockRenderer() : base(MinecraftLibrary.Engine.Blocks.Block.GetBlock(BlockType.Sapling))
    {
        Textures.Add(Texture.LoadTexture("Render/Textures/Sapling.dds"));
        DrawType = DrawType.Cross;
    }

    protected override uint GetIndexTexture(BlockFaces face)
    {
        return Textures[0].TextureIndex;
    }
}