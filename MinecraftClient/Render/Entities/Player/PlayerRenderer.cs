using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.Entities.Player;

public sealed class PlayerRenderer(PlayerState player)
{
    private PlayerState Player { get; } = player;
    private PlayerState OldPlayerState { get; } = new(0);

    public void UpdateRenderer(float delta)
    {
        Camera.GetInstance().Position = OldPlayerState.Position + (Player.Position - OldPlayerState.Position) * delta + Vector3.UnitY * (EngineDefaults.CameraOffset - EngineDefaults.PlayerSize.Y);
        Camera.GetInstance().Pitch = OldPlayerState.Pitch + (Player.Pitch - OldPlayerState.Pitch) * delta;
        Camera.GetInstance().Yaw = OldPlayerState.Rotation.Y + (Player.Rotation.Y - OldPlayerState.Rotation.Y) * delta;
    }

    public void ApplyRevertChanges(Packet changePacket)
    {
        OldPlayerState.DeserializeChanges(changePacket);
    }
    
    public ushort GetPlayerId()
    {
        return Player.EntityId;
    }
}