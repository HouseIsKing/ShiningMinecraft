using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Blocks;

public sealed class SaplingBlock() : Block(BlockType.Sapling)
{
    public override void Tick(World world, Vector3i pos)
    {
        base.Tick(world, pos);
        var blockTypeBelow = world.GetBlockAt(pos - Vector3i.UnitY).Type;
        if (blockTypeBelow is BlockType.Dirt or BlockType.Grass && world.GetBrightnessAt(pos) == 1)
            return;

        world.SetBlockAt(pos, BlockType.Air);
    }
}