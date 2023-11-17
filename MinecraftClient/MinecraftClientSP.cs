using MinecraftLibrary.Engine;

namespace MinecraftClient;

public class MinecraftClientSP : MinecraftClient
{
    public MinecraftClientSP() : base(new World())
    {
    }
}