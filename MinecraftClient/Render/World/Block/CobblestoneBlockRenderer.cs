using MinecraftClient.Render.Textures;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;

namespace MinecraftClient.Render.World.Block;

public class CobblestoneBlockRenderer : BlockRenderer
{
    public CobblestoneBlockRenderer() : base(MinecraftLibrary.Engine.Blocks.Block.GetBlock(BlockType.Cobblestone))
    {
        Textures.Add(Texture.LoadTexture("Render/Textures/Cobblestone.dds"));
    }

    protected override uint GetIndexTexture(BlockFaces face)
    {
        return Textures[0].TextureIndex;
    }
}