using System.Diagnostics;
using System.Net;

namespace MinecraftClient;

file static class Program
{
    public static void Main(string[] args)
    {
        using var p = Process.GetCurrentProcess();
        {
            p.PriorityClass = ProcessPriorityClass.AboveNormal;
        }
        if (args.Length > 2 && args[0] == "Server")
        {
            MinecraftClientMp server = new(IPAddress.Parse(args[1]), args[2]);
            server.Run();
        }
        else
        {
            MinecraftClientSp client = new();
            client.Run();
        }
    }
}