using MinecraftClient.Render.Entities.Player;
using MinecraftClient.Render.World;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Input;
using OpenTK.Mathematics;

namespace MinecraftClient;

public sealed class MinecraftClientSp : MinecraftClient
{
    private ulong _inputId;
    public MinecraftClientSp() : base(new World())
    {
        Player = World.GetInstance()!.SpawnPlayer();
        Player.Position = new Vector3(5, 67, 5);
        WorldRenderer = new WorldRenderer(new PlayerRenderer(Player));
        World.GenerateLevel();
        Console.WriteLine("Done loading world");
    }

    protected override void PreTick()
    {
        Input.MouseX *= 0.4f;
        Input.MouseY *= 0.4f;
        World.GetInstance()?.GetPlayer(Player.EntityId).AddInput(_inputId++, Input);
        Input = new ClientInput();
    }

    protected override void PostTick()
    {
    }
}