using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Blocks;

public class Block
{
    private static readonly Block[] Blocks =
    {
        new AirBlock(), new GrassBlock(), new DirtBlock(), new CobblestoneBlock(), new StoneBlock(), new PlanksBlock(),
        new SaplingBlock()
    };
    
    public static Block GetBlock(BlockType type)
    {
        return Blocks[(int)type];
    }
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

    public virtual void Tick(World world, Vector3i pos)
    {
    }

    public virtual void OnBreak(World world, Vector3i pos)
    {
        for (var i = 0; i < 4; i++)
        for (var j = 0; j < 4; j++)
        for (var k = 0; k < 4; k++)
        {
            Random random = world.GetWorldRandom();
            var particle = world.SpawnBlockParticleEntity();
            particle.BlockParticleType = Type;
            var offset = (new Vector3(i, j, k) + new Vector3(0.5f)) / 4.0f;
            particle.Position = pos + offset;
            var velocityDirection = offset - new Vector3(0.5f) + (new Vector3(random.NextFloat(), j, k) * 2.0f - Vector3.One) * 0.4f;
            velocityDirection.NormalizeFast();
            var velocityForce = (random.NextFloat() + random.NextFloat() + 1.0f) * 0.15f;
            particle.Velocity = velocityDirection * velocityForce * new Vector3(0.7f, 1.0f, 0.7f);
            particle.Scale = EngineDefaults.ParticleSize * (random.NextFloat() * 0.5f + 0.5f);
            particle.MaxLifeTime = (byte)(4.0f / random.NextFloat() * 0.9f + 0.1f);
        }
    }
}