using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.Entities;

public abstract class EntityState<TEntityType> : State<TEntityType> where TEntityType : EntityState<TEntityType>
{
    private readonly (bool, object)[] _changes = new (bool, object)[Enum.GetValues<EEntityChange>().Length];
    private byte _changesCount;
    private bool _isGrounded;
    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _scale;
    private Vector3 _velocity;
    public ushort EntityId { get; }
    public EntityType EntityType { get; }
    public bool DidSpawn { get; internal set; }

    protected EntityState(ushort id, EntityType type)
    {
        EntityType = type;
        EntityId = id;
        for (var i = 0; i < _changes.Length; i++) _changes[i] = (false, id);
    }

    public Vector3 Position
    {
        get => _position;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EEntityChange.Position].Item1)
            {
                _changes[(byte)EEntityChange.Position].Item1 = true;
                _changes[(byte)EEntityChange.Position].Item2 = _position;
                _changesCount++;
            }

            _position = value;
        }
    }

    public Vector3 Rotation
    {
        get => _rotation;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EEntityChange.Rotation].Item1)
            {
                _changes[(byte)EEntityChange.Rotation].Item1 = true;
                _changes[(byte)EEntityChange.Rotation].Item2 = _rotation;
                _changesCount++;
            }

            _rotation = value;
        }
    }

    public Vector3 Scale
    {
        get => _scale;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EEntityChange.Scale].Item1)
            {
                _changes[(byte)EEntityChange.Scale].Item1 = true;
                _changes[(byte)EEntityChange.Scale].Item2 = _scale;
                _changesCount++;
            }

            _scale = value;
        }
    }

    public Vector3 Velocity
    {
        get => _velocity;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EEntityChange.Velocity].Item1)
            {
                _changes[(byte)EEntityChange.Velocity].Item1 = true;
                _changes[(byte)EEntityChange.Velocity].Item2 = _velocity;
                _changesCount++;
            }

            _velocity = value;
        }
    }

    public bool IsGrounded
    {
        get => _isGrounded;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EEntityChange.IsGrounded].Item1)
            {
                _changes[(byte)EEntityChange.IsGrounded].Item1 = true;
                _changes[(byte)EEntityChange.IsGrounded].Item2 = _isGrounded;
                _changesCount++;
            }

            _isGrounded = value;
        }
    }

    public override void Serialize(Packet packet)
    {
        packet.Write(EntityId);
        packet.Write((byte)EntityType);
        packet.Write(Position);
        packet.Write(Rotation);
        packet.Write(Scale);
        packet.Write(Velocity);
        packet.Write(IsGrounded);
    }

    public override void Deserialize(Packet packet)
    {
        packet.Read(out _position);
        packet.Read(out _rotation);
        packet.Read(out _scale);
        packet.Read(out _velocity);
        packet.Read(out _isGrounded);
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
        switch ((EEntityChange)change)
        {
            case EEntityChange.Position:
                changePacket.Write(Position);
                break;
            case EEntityChange.Rotation:
                changePacket.Write(Rotation);
                break;
            case EEntityChange.Scale:
                changePacket.Write(Scale);
                break;
            case EEntityChange.Velocity:
                changePacket.Write(Velocity);
                break;
            case EEntityChange.IsGrounded:
                changePacket.Write(IsGrounded);
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
        switch ((EEntityChange)change)
        {
            case EEntityChange.Position or EEntityChange.Rotation or EEntityChange.Scale or EEntityChange.Velocity:
                revertPacket.Write((Vector3)_changes[change].Item2);
                break;
            case EEntityChange.IsGrounded:
                revertPacket.Write((bool)_changes[change].Item2);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, "Invalid Entity State Change");
        }
    }

    public override void DeserializeChanges(Packet changePacket)
    {
        changePacket.Read(out byte changeCount);
        for (var i = 0; i < changeCount; i++)
        {
            changePacket.Read(out byte change);
            DeserializeChange(changePacket, change);
        }
    }

    private void DeserializeChange(Packet packet, byte change)
    {
        switch ((EEntityChange)change)
        {
            case EEntityChange.Position:
                packet.Read(out _position);
                break;
            case EEntityChange.Rotation:
                packet.Read(out _rotation);
                break;
            case EEntityChange.Scale:
                packet.Read(out _scale);
                break;
            case EEntityChange.Velocity:
                packet.Read(out _velocity);
                break;
            case EEntityChange.IsGrounded:
                packet.Read(out _isGrounded);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, "Invalid Entity State Change");
        }
    }
}