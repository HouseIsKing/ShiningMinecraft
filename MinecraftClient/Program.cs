namespace MinecraftClient;

public abstract class Program
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
            MinecraftClientSP client = new();
            client.Run();
        }
    }
}