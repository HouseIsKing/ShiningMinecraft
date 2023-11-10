namespace MinecraftLibrary.Engine;

public abstract class World
{
    private static World? Instance;
    public static World? GetInstance()
    {
        return Instance;
    }

    ~World()
    {
        Instance = null;
    }

    public abstract void Run();
    public abstract void NewTick();

    public virtual void PreTick()
    {
    }
}