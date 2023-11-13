using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.Entities;

public abstract class EntityState<TEntityType> : State<TEntityType> where TEntityType : EntityState<TEntityType>
{
    public event EngineDefaults.EntityUpdateHandler OnEntityUpdate;
    private bool _isGrounded;
    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _scale;
    private Vector3 _velocity;
    public ushort EntityId { get; }
    public EntityType EntityType { get; }
    protected EntityState(ushort id, EntityType type)
    {
        EntityType = type;
        EntityId = id;
    }

    public Vector3 Position
    {
        get => _position;
        set
        {
            Changes.TryAdd((ushort)StateChange.EntityPosition, _position);
            TriggerEntityUpdate();
            _position = value;
        }
    }

    public Vector3 Rotation
    {
        get => _rotation;
        set
        {
            Changes.TryAdd((ushort)StateChange.EntityRotation, _rotation);
            TriggerEntityUpdate();
            _rotation = value;
        }
    }

    public Vector3 Scale
    {
        get => _scale;
        set
        {
            Changes.TryAdd((ushort)StateChange.EntityScale, _scale);
            TriggerEntityUpdate();
            _scale = value;
        }
    }

    public Vector3 Velocity
    {
        get => _velocity;
        set
        {
            Changes.TryAdd((ushort)StateChange.EntityVelocity, _velocity);
            TriggerEntityUpdate();
            _velocity = value;
        }
    }

    public bool IsGrounded
    {
        get => _isGrounded;
        set
        {
            Changes.TryAdd((ushort)StateChange.EntityIsGrounded, _isGrounded);
            TriggerEntityUpdate();
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

    protected override void SerializeChangeToChangePacket(Packet changePacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.EntityPosition:
                changePacket.Write(Position);
                break;
            case StateChange.EntityRotation:
                changePacket.Write(Rotation);
                break;
            case StateChange.EntityScale:
                changePacket.Write(Scale);
                break;
            case StateChange.EntityVelocity:
                changePacket.Write(Velocity);
                break;
            case StateChange.EntityIsGrounded:
                changePacket.Write(IsGrounded);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, "Invalid Entity State Change");
        }
    }

    protected override void SerializeChangeToRevertPacket(Packet revertPacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.EntityPosition or StateChange.EntityRotation or StateChange.EntityScale or StateChange.EntityVelocity:
                revertPacket.Write((Vector3)Changes[change]);
                break;
            case StateChange.EntityIsGrounded:
                revertPacket.Write((bool)Changes[change]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, "Invalid Entity State Change");
        }
    }

    protected override void DeserializeChange(Packet packet, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.EntityPosition:
                packet.Read(out _position);
                break;
            case StateChange.EntityRotation:
                packet.Read(out _rotation);
                break;
            case StateChange.EntityScale:
                packet.Read(out _scale);
                break;
            case StateChange.EntityVelocity:
                packet.Read(out _velocity);
                break;
            case StateChange.EntityIsGrounded:
                packet.Read(out _isGrounded);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, "Invalid Entity State Change");
        }
    }
    
    protected void TriggerEntityUpdate()
    {
        OnEntityUpdate.Invoke(EntityId);
    }
}