using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.Entities.Player;

public sealed class PlayerRenderer(PlayerState player)
{
    private PlayerState Player { get; } = player;
    private PlayerState OldPlayerState { get; } = new(0);
    private readonly SelectionHighlightTessellator _selectionHighlightTessellator = new();

    public void UpdateCamera(float delta)
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
        var world = MinecraftLibrary.Engine.World.Instance;
        if (!Player.DidSpawn) return;
        var p = world.GetPlayer(Player.EntityId);
        if (!p.FoundBlock) return;
        _selectionHighlightTessellator.Mode = p.State.Mode;
        if (p.State.Mode)
        {
            var pos = Vector3i.Zero;
            switch (p.HitFace)
            {
                case BlockFaces.Bottom:
                    pos = p.HitPosition - Vector3i.UnitY;
                    break;
                case BlockFaces.Top:
                    pos = p.HitPosition + Vector3i.UnitY;
                    break;
                case BlockFaces.North:
                    pos = p.HitPosition + Vector3i.UnitZ;
                    break;
                case BlockFaces.South:
                    pos = p.HitPosition - Vector3i.UnitZ;
                    break;
                case BlockFaces.East:
                    pos = p.HitPosition + Vector3i.UnitX;
                    break;
                case BlockFaces.West:
                    pos = p.HitPosition - Vector3i.UnitX;
                    break;
            }
            _selectionHighlightTessellator.SetVertex(p.State.CurrentSelectedBlock, BuildLightBlock(pos), pos);
        }
        else
        {
            _selectionHighlightTessellator.SetVertex(world.GetBlockAt(p.HitPosition).Type, BuildLightBlock(p.HitPosition), p.HitPosition);
        }
        _selectionHighlightTessellator.Draw(1);
    }

    private static byte BuildLightBlock(Vector3i pos)
    {
        var world = MinecraftLibrary.Engine.World.Instance;
        var result = world.GetBrightnessAt(pos + Vector3i.UnitY);
        result |= (byte)(world.GetBrightnessAt(pos + Vector3i.UnitX) << 2);
        result |= (byte)(world.GetBrightnessAt(pos - Vector3i.UnitX) << 3);
        result |= (byte)(world.GetBrightnessAt(pos + Vector3i.UnitZ) << 4);
        result |= (byte)(world.GetBrightnessAt(pos - Vector3i.UnitZ) << 5);
        return result;
    }
}