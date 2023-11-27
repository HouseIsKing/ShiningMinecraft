using MinecraftClient.Render.Textures;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;

namespace MinecraftClient.Render.World.Block;

public class DirtBlockRenderer : BlockRenderer
{
    public DirtBlockRenderer() : base(MinecraftLibrary.Engine.Blocks.Block.GetBlock(BlockType.Dirt))
    {
        Textures.Add(Texture.LoadTexture("Render/Textures/Dirt.dds"));
    }

    protected override uint GetIndexTexture(BlockFaces face)
    {
        return Textures[0].TextureIndex;
    }
}