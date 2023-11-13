using MinecraftLibrary.Engine.States.World;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine;

public abstract class World
{
    private static World? _instance;
    public WorldState State { get; }
    public static World? GetInstance()
    {
        return _instance;
    }

    protected World(long seed)
    {
        if (_instance != null) throw new Exception("World already exists");

        State = new WorldState(seed);
        _instance = this;
    }

    ~World()
    {
        _instance = null;
    }

    public abstract void Run();
    public abstract void NewTick();

    public virtual void PreTick()
    {
    }

    public virtual void Tick()
    {
        
    }

    public List<Box3> GetBlocksColliding(Box3 collider)
    {
        Vector3i min = new Vector3i((int)collider.Min.X, (int)collider.Min.Y, (int)collider.Min.Z);
        Vector3i max = new Vector3i((int)collider.Max.X + 1, (int)collider.Max.Y + 1, (int)collider.Max.Z + 1);
        var blocks = new List<Box3>();
        if (collider.Min.X < 0) min.X -= 1;
        if (collider.Min.Y < 0) min.Y -= 1;
        if (collider.Min.Z < 0) min.Z -= 1;
        for (int x = min.X; x < max.X; x++)
        {
            for(int y = min.Y; y < max.Y; y++)
            {
                for(int z = min.Z; z < max.Z; z++)
                {
                    var block = 
                    if (block.IsSolid())
                    {
                        blocks.Add(new Box3(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1)));
                    }
                }
            }
        }
    }
}