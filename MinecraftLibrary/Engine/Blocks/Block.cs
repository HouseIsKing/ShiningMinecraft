using MinecraftLibrary.Network;
using MinecraftLibrary.Engine;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Blocks;

public class Block
{
    public BlockType Type { get; private set; }
    public Box3 BlockBounds { get; private set; }

    protected Block(BlockType type, Box3 blockBounds)
    {
        Type = type;
        BlockBounds = blockBounds;
    }

    protected Block(BlockType type) : this(type, new Box3(Vector3.Zero, Vector3.One))
    {
    }

    public virtual bool IsSolid()
    {
        return true;
    }

    public virtual bool IsBlockingLight()
    {
        return true;
    }

    public virtual void Tick(World world, int x, int y, int z)
    {
    }

    public virtual void OnBreak(World world, int x, int y, int z)
    {
    }
}