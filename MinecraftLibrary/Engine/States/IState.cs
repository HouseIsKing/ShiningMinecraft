using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States;

public interface IState
{
    public void Serialize(Packet packet);
    public void Deserialize(Packet packet);
}