using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Network;

namespace MinecraftClient.Render.Entities.Player;

public class OtherPlayerRenderer(PlayerState player)
{
    private PlayerState Player { get; } = player;
    private PlayerState OldPlayerState { get; } = new(0);

    public void ApplyRevertChanges(Packet changePacket)
    {
        OldPlayerState.DeserializeChanges(changePacket);
    }
    
    public ushort GetPlayerId()
    {
        return Player.EntityId;
    }
}