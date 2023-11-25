using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;

namespace MinecraftClient.Render.World.Block;

internal sealed class AirBlockRenderer() : BlockRenderer(MinecraftLibrary.Engine.Blocks.Block.GetBlock(BlockType.Air))
{
    protected override uint GetIndexTexture(BlockFaces face)
    {
        return 0;
    }
}