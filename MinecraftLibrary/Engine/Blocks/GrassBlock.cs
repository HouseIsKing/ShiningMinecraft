using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Blocks;

public sealed class GrassBlock() : Block(BlockType.Grass)
{
    public override void Tick(World world, Vector3i pos)
    {
        base.Tick(world, pos);
        if (world.GetBrightnessAt(pos + Vector3i.UnitY) == 1)
            for (var i = 0; i < 4; i++)
            {
                var random = world.GetWorldRandom();
                var offset = new Vector3i(random.NextInt(3) - 1, random.NextInt(5) - 3, random.NextInt(3) - 1);
                var finalPos = pos + offset;
                if (world.GetBrightnessAt(finalPos + Vector3i.UnitY) == 1 &&
                    world.GetBlockAt(finalPos).Type == BlockType.Dirt)
                    world.SetBlockAt(finalPos, BlockType.Grass);
            }
        else
            world.SetBlockAt(pos, BlockType.Dirt);
    }
}