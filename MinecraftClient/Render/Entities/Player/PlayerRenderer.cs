using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.Entities.Player;

public sealed class PlayerRenderer(PlayerState player)
{
    private PlayerState Player { get; } = player;
    private PlayerState OldPlayerState { get; } = new(0);
    private readonly SelectionHighlightTessellator _selectionHighlightTessellator = new();

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

    public void RenderSelectionHighlight()
    {
        var world = MinecraftLibrary.Engine.World.GetInstance();
        if (world == null || !Player.DidSpawn) return;
        var p = world.GetPlayer(Player.EntityId);
        if (p.State.Mode || !p.FoundBlock) return;
        _selectionHighlightTessellator.SetVertex(world.GetBlockAt(p.HitPosition).Type, world.GetBrightnessAt(p.HitPosition), p.HitPosition);
        _selectionHighlightTessellator.Draw();
    }
}