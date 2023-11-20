using System.Collections;
using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States;

public abstract class State<TStateType> where TStateType : State<TStateType>
{
    public bool IsDirty { get; protected set; }
    public abstract void Serialize(Packet packet);
    public abstract void Deserialize(Packet packet);
    public abstract void SerializeChangesToChangePacket(Packet changePacket);
    public abstract void SerializeChangesToRevertPacket(Packet revertPacket);
    public abstract void DeserializeChanges(Packet changePacket);

    public virtual void FinalizeChanges()
    {
        IsDirty = false;
    }
}