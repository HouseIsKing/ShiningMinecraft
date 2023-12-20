using MinecraftClient.Render.Textures;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;

namespace MinecraftClient.Render.World.Block;

internal sealed class GrassBlockRenderer : BlockRenderer
{
    public GrassBlockRenderer() : base(MinecraftLibrary.Engine.Blocks.Block.GetBlock(BlockType.Grass))
    {
        Textures.Add(Texture.LoadTexture("Render/Textures/GrassTop.dds"));
        Textures.Add(Texture.LoadTexture("Render/Textures/GrassSide.dds"));
        Textures.Add(Texture.LoadTexture("Render/Textures/Dirt.dds"));
    }

    protected override uint GetIndexTexture(BlockFaces face)
    {
        return face switch
        {
            BlockFaces.Bottom => Textures[2].TextureIndex,
            BlockFaces.Top => Textures[0].TextureIndex,
            _ => Textures[1].TextureIndex
        };
    }

    protected override uint GetColor(BlockFaces face)
    {
        return face == BlockFaces.Top ? 0x69a93fff : base.GetColor(face);
    }
}