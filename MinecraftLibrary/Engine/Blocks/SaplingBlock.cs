using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Blocks;

internal sealed class SaplingBlock() : Block(BlockType.Sapling)
{
    public override void Tick(World world, Vector3i pos)
    {
        base.Tick(world, pos);
        var blockTypeBelow = world.GetBlockAt(pos - Vector3i.UnitY).Type;
        if (blockTypeBelow is BlockType.Dirt or BlockType.Grass && world.GetBrightnessAt(pos) == 1)
            return;

        world.SetBlockAt(pos, BlockType.Air);
    }

    public override bool IsSolid()
    {
        return false;
    }

    public override bool IsBlockingLight()
    {
        return false;
    }
}