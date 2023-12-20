using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Blocks;

public sealed class AirBlock() : Block(BlockType.Air, new Box3())
{
    public override bool IsSolid()
    {
        return false;
    }

    public override bool IsBlockingLight()
    {
        return false;
    }
}