using System.Diagnostics;

namespace MinecraftClient;

file static class Program
{
    public static void Main(string[] args)
    {
        using var p = Process.GetCurrentProcess();
        {
            p.PriorityClass = ProcessPriorityClass.AboveNormal;
        }
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