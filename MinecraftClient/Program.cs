namespace MinecraftClient;

file static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 3 && args[1] == "Server")
        {
            MinecraftClientMP server = new();
            server.Run();
        }
        else
        {
            MinecraftClientSp client = new();
            client.Run();
        }
    }
}