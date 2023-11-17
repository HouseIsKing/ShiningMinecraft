using MinecraftLibrary.Engine.Blocks;

namespace MinecraftClient.Render.World.Block;

public class AirBlockRenderer : BlockRenderer
{
    protected override ushort GetIndexTexture(BlockFaces face)
    {
        return 0;
    }
}