using MinecraftLibrary.Engine;
using MinecraftLibrary.Input;
using OpenTK.Mathematics;

namespace MinecraftClient;

public sealed class MinecraftClientSp : MinecraftClient
{
    private ulong _inputId;
    public MinecraftClientSp() : base(new World())
    {
        Player.Position = new Vector3(5, 67, 5);
        World.GenerateLevel();
        Console.WriteLine("Done loading world");
    }

    protected override void PreTick()
    {
        if (!Player.DidSpawn) return;
        Input.MouseX *= 0.4f;
        Input.MouseY *= 0.4f;
        World.GetInstance()?.GetPlayer(Player.EntityId).AddInput(_inputId++, Input);
        Input = new ClientInput();
    }

    protected override void PostTick()
    {
    }
}