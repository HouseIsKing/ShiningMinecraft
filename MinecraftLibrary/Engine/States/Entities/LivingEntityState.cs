using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States.Entities;

public abstract class LivingEntityState<TLivingEntityState> : EntityState<TLivingEntityState> where TLivingEntityState : LivingEntityState<TLivingEntityState>
{
    private readonly (bool, object)[] _changes = new (bool, object)[Enum.GetValues<ELivingEntityChange>().Length];
    private byte _changesCount;
    private bool _jumpInput;
    private float _horizontalInput;
    private float _verticalInput;
    
    public bool JumpInput
    {
        get => _jumpInput;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)ELivingEntityChange.JumpInput].Item1)
            {
                _changes[(byte)ELivingEntityChange.JumpInput].Item1 = true;
                _changes[(byte)ELivingEntityChange.JumpInput].Item2 = _jumpInput;
                _changesCount++;
            }

            _jumpInput = value;
        }
    }
    
    public float HorizontalInput
    {
        get => _horizontalInput;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)ELivingEntityChange.HorizontalInput].Item1)
            {
                _changes[(byte)ELivingEntityChange.HorizontalInput].Item1 = true;
                _changes[(byte)ELivingEntityChange.HorizontalInput].Item2 = _horizontalInput;
                _changesCount++;
            }

            _horizontalInput = value;
        }
    }
    
    public float VerticalInput
    {
        get => _verticalInput;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)ELivingEntityChange.VerticalInput].Item1)
            {
                _changes[(byte)ELivingEntityChange.VerticalInput].Item1 = true;
                _changes[(byte)ELivingEntityChange.VerticalInput].Item2 = _verticalInput;
                _changesCount++;
            }

            _verticalInput = value;
        }
    }

    protected LivingEntityState(ushort id, EntityType type) : base(id, type)
    {
        for (var i = 0; i < _changes.Length; i++) _changes[i] = (false, id);
    }

    public override void Serialize(Packet packet)
    {
        base.Serialize(packet);
        packet.Write(JumpInput);
        packet.Write(HorizontalInput);
        packet.Write(VerticalInput);
    }

    public override void Deserialize(Packet packet)
    {
        base.Deserialize(packet);
        packet.Read(out _jumpInput);
        packet.Read(out _horizontalInput);
        packet.Read(out _verticalInput);
    }

    public override void SerializeChangesToChangePacket(Packet changePacket)
    {
        base.SerializeChangesToChangePacket(changePacket);
        changePacket.Write(_changesCount);
        for (byte i = 0; i < _changes.Length; i++)
            if (_changes[i].Item1)
            {
                changePacket.Write(i);
                SerializeChangeToChangePacket(changePacket, i);
                _changes[i].Item1 = false;
            }
        _changesCount = 0;
    }

    private void SerializeChangeToChangePacket(Packet changePacket, byte change)
    {
        switch ((ELivingEntityChange)change)
        {
            case ELivingEntityChange.HorizontalInput:
                changePacket.Write(HorizontalInput);
                break;
            case ELivingEntityChange.VerticalInput:
                changePacket.Write(VerticalInput);
                break;
            case ELivingEntityChange.JumpInput:
                changePacket.Write(JumpInput);
                break;
        }
    }

    public override void SerializeChangesToRevertPacket(Packet revertPacket)
    {
        base.SerializeChangesToRevertPacket(revertPacket);
        revertPacket.Write(_changesCount);
        for (byte i = 0; i < _changes.Length; i++)
            if (_changes[i].Item1)
            {
                revertPacket.Write(i);
                SerializeChangeToRevertPacket(revertPacket, i);
                _changes[i].Item1 = false;
            }
        _changesCount = 0;
    }

    private void SerializeChangeToRevertPacket(Packet revertPacket, byte change)
    {
        switch ((ELivingEntityChange)change)
        {
            case ELivingEntityChange.HorizontalInput or ELivingEntityChange.VerticalInput:
                revertPacket.Write((float)_changes[change].Item2);
                break;
            case ELivingEntityChange.JumpInput:
                revertPacket.Write((bool)_changes[change].Item2);
                break;
        }
    }

    public override void DeserializeChanges(Packet changePacket)
    {
        base.DeserializeChanges(changePacket);
        changePacket.Read(out byte changeCount);
        for (var i = 0; i < changeCount; i++)
        {
            changePacket.Read(out byte change);
            DeserializeChange(changePacket, change);
        }
    }

    private void DeserializeChange(Packet packet, byte change)
    {
        switch ((ELivingEntityChange)change)
        {
            case ELivingEntityChange.JumpInput:
                packet.Read(out _jumpInput);
                JumpInput = _jumpInput;
                break;
            case ELivingEntityChange.HorizontalInput:
                packet.Read(out _horizontalInput);
                HorizontalInput = _horizontalInput;
                break;
            case ELivingEntityChange.VerticalInput:
                packet.Read(out _verticalInput);
                VerticalInput = _verticalInput;
                break;
        }
    }

    public override void DiscardChanges()
    {
        base.DiscardChanges();
        _changesCount = 0;
        for (byte i = 0; i < _changes.Length; i++) _changes[i].Item1 = false;
    }
}