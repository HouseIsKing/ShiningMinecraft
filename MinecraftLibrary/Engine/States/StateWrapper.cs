using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States;

public abstract class StateWrapper<TStateType, TChangeType> where TStateType : IState, new() where TChangeType : Enum
{
    private HashSet<TChangeType> Changes = new();
    public TStateType State { get; protected set; } = new();
    public TStateType PreviousState { get; protected set; } = new();

    protected abstract void SerializeChangeToPacket(Packet packet, TChangeType change);
    protected abstract void FinalizeChange(TChangeType change);
    protected abstract void DeserializeChange(Packet packet, TChangeType change);

    public void SerializeChangesToPacket(Packet packet)
    {
        packet.Write((ushort)Changes.Count);
        foreach (var change in Changes)
        {
            packet.Write(Convert.ToByte(change));
            SerializeChangeToPacket(packet, change);
        }
    }

    public void FinalizeChanges()
    {
        foreach (var change in Changes) FinalizeChange(change);

        Changes.Clear();
    }

    public void DeserializeChanges(Packet packet)
    {
        packet.Read(out ushort changeCount);
        for (ushort i = 0; i < changeCount; i++)
        {
            packet.Read(out byte change);
            DeserializeChange(packet, (TChangeType)Enum.ToObject(typeof(TChangeType), change));
        }
    }
}