namespace MinecraftServer;

file static class MinecraftServer
{
    public static void Main()
    {
        var serverManager = new ServerManager();
        while (true) serverManager.Run();
    }
}