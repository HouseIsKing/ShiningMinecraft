namespace MinecraftLibrary.Network;

public enum PacketType : uint
{
    PlayerId,
    WorldChange,
    WorldState,
    ClientInput,
    SaveWorld,
    LoadWorld
}