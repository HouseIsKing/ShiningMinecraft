using MinecraftClient.Render.Textures;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;

namespace MinecraftClient.Render.World.Block;

internal sealed class PlanksBlockRenderer : BlockRenderer
{
    internal PlanksBlockRenderer() : base(MinecraftLibrary.Engine.Blocks.Block.GetBlock(BlockType.Planks))
    {
        Textures.Add(Texture.LoadTexture("Render/Textures/Planks.dds"));
    }
    protected override uint GetIndexTexture(BlockFaces face)
    {
        return Textures[0].TextureIndex;
    }
}