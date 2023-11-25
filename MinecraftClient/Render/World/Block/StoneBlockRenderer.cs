using MinecraftClient.Render.Textures;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;

namespace MinecraftClient.Render.World.Block;

internal sealed class StoneBlockRenderer : BlockRenderer
{
    public StoneBlockRenderer() : base(MinecraftLibrary.Engine.Blocks.Block.GetBlock(BlockType.Stone))
    {
        Textures.Add(Texture.LoadTexture("Render/Textures/Stone.dds"));
    }
    protected override uint GetIndexTexture(BlockFaces face)
    {
        return Textures[0].TextureIndex;
    }
}