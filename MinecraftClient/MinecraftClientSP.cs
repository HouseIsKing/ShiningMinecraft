using MinecraftLibrary.Engine;
using MinecraftLibrary.Input;
using OpenTK.Mathematics;

namespace MinecraftClient;

public sealed class MinecraftClientSp : MinecraftClient
{
    public MinecraftClientSp() : base(new World())
    {
        Player = World.SpawnPlayer();
        Player.Position = new Vector3(5, 67, 5);
        WorldRenderer.PlayerRenderer.Player = Player;
        World.GenerateLevel();
        Console.WriteLine("Done loading world");
    }
}