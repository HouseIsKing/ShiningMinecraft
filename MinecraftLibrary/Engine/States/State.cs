using System.Collections;
using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States;

public abstract class State<TStateType> where TStateType : State<TStateType>
{
    public event EngineDefaults.StateChangedHandler OnChange;
    private bool _isDirty;
    public bool IsDirty
    {
        get => _isDirty;
        protected set
        {
            if (!_isDirty && value) OnChange?.Invoke();
            _isDirty = value;
        }
    }
    public abstract void Serialize(Packet packet);
    public abstract void Deserialize(Packet packet);

    public virtual void SerializeChangesToChangePacket(Packet changePacket)
    {
        IsDirty = false;
    }

    public virtual void SerializeChangesToRevertPacket(Packet revertPacket)
    {
        IsDirty = false;
    }
    public abstract void DeserializeChanges(Packet changePacket);

    public virtual void DiscardChanges()
    {
        IsDirty = false;
    }
}