namespace MinecraftServer;

file static class MinecraftServer
{
    public static void Main(string[] args)
    {
        var serverManager = new ServerManager();
        while (true) serverManager.Run();
    }
}