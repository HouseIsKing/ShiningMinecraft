using System.Collections;
using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States;

public abstract class State<TStateType> where TStateType : State<TStateType>
{
    protected SortedDictionary<ushort, object> Changes { get; } = new();

    public abstract void Serialize(Packet packet);
    public abstract void Deserialize(Packet packet);
    protected abstract void SerializeChangeToChangePacket(Packet changePacket, ushort change);
    protected abstract void SerializeChangeToRevertPacket(Packet revertPacket, ushort change);
    protected abstract void DeserializeChange(Packet changePacket, ushort change);

    public void SerializeChangesToChangePacket(Packet changePacket)
    {
        changePacket.Write((ushort)Changes.Count);
        foreach (var change in Changes.Keys)
        {
            changePacket.Write(Convert.ToUInt16(change));
            SerializeChangeToChangePacket(changePacket, change);
        }
    }
    
    public void SerializeChangesToRevertPacket(Packet revertPacket)
    {
        revertPacket.Write((ushort)Changes.Count);
        foreach (var change in Changes.Keys)
        {
            revertPacket.Write(Convert.ToUInt16(change));
            SerializeChangeToRevertPacket(revertPacket, change);
        }
    }

    public virtual void FinalizeChanges()
    {
        Changes.Clear();
    }

    public void DeserializeChanges(Packet changePacket)
    {
        changePacket.Read(out ushort changeCount);
        for (ushort i = 0; i < changeCount; i++)
        {
            changePacket.Read(out ushort change);
            DeserializeChange(changePacket, change);
        }
    }
}