using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States.Entities;

public sealed class BlockParticleEntityState : EntityState<BlockParticleEntityState>
{
    private readonly (bool, object)[] _changes = new (bool, object)[Enum.GetValues<EBlockParticleChange>().Length];
    private byte _changesCount;
    private BlockType _blockParticleType = BlockType.Air;
    private byte _lifeTime;
    private byte _maxLifeTime;
    
    public byte LifeTime
    {
        get => _lifeTime;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EBlockParticleChange.LifeTime].Item1)
            {
                _changes[(byte)EBlockParticleChange.LifeTime].Item1 = true;
                _changes[(byte)EBlockParticleChange.LifeTime].Item2 = _lifeTime;
                _changesCount++;
            }

            _lifeTime = value;
        }
    }
    
    public byte MaxLifeTime
    {
        get => _maxLifeTime;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EBlockParticleChange.MaxLifeTime].Item1)
            {
                _changes[(byte)EBlockParticleChange.MaxLifeTime].Item1 = true;
                _changes[(byte)EBlockParticleChange.MaxLifeTime].Item2 = _maxLifeTime;
                _changesCount++;
            }

            _maxLifeTime = value;
        }
    }
    
    public BlockType BlockParticleType
    {
        get => _blockParticleType;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EBlockParticleChange.Type].Item1)
            {
                _changes[(byte)EBlockParticleChange.Type].Item1 = true;
                _changes[(byte)EBlockParticleChange.Type].Item2 = _blockParticleType;
                _changesCount++;
            }

            _blockParticleType = value;
        }
    }

    public BlockParticleEntityState(ushort id) : base(id, EntityType.BlockBreakParticle)
    {
        for (var i = 0; i < _changes.Length; i++) _changes[i] = (false, id);
    }

    public override void Serialize(Packet packet)
    {
        base.Serialize(packet);
        packet.Write(LifeTime);
        packet.Write(MaxLifeTime);
        packet.Write((byte)BlockParticleType);
    }

    public override void Deserialize(Packet packet)
    {
        base.Deserialize(packet);
        packet.Read(out _lifeTime);
        packet.Read(out _maxLifeTime);
        packet.Read(out byte helper);
        _blockParticleType = (BlockType)helper;
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
        switch ((EBlockParticleChange)change)
        {
            case EBlockParticleChange.LifeTime:
                changePacket.Write(LifeTime);
                break;
            case EBlockParticleChange.MaxLifeTime:
                changePacket.Write(MaxLifeTime);
                break;
            case EBlockParticleChange.Type:
                changePacket.Write((byte)BlockParticleType);
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
        switch ((EBlockParticleChange)change)
        {
            case EBlockParticleChange.LifeTime:
                revertPacket.Write((byte)_changes[change].Item2);
                break;
            case EBlockParticleChange.MaxLifeTime:
                revertPacket.Write((byte)_changes[change].Item2);
                break;
            case EBlockParticleChange.Type:
                revertPacket.Write((byte)_changes[change].Item2);
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
        switch ((EBlockParticleChange)change)
        {
            case EBlockParticleChange.Type:
                packet.Read(out byte helper);
                BlockParticleType = (BlockType)helper;
                break;
            case EBlockParticleChange.LifeTime:
                packet.Read(out _lifeTime);
                LifeTime = _lifeTime;
                break;
            case EBlockParticleChange.MaxLifeTime:
                packet.Read(out _maxLifeTime);
                MaxLifeTime = _maxLifeTime;
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