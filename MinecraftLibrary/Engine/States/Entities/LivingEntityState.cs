using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States.Entities;

public abstract class LivingEntityState<TLivingEntityState> : EntityState<TLivingEntityState> where TLivingEntityState : LivingEntityState<TLivingEntityState>
{
    private bool _jumpInput;
    private float _horizontalInput;
    private float _verticalInput;
    
    public bool JumpInput
    {
        get => _jumpInput;
        set
        {
            Changes.TryAdd((ushort)StateChange.LivingEntityJumpInput, _jumpInput);
            TriggerEntityUpdate();
            _jumpInput = value;
        }
    }
    
    public float HorizontalInput
    {
        get => _horizontalInput;
        set
        {
            Changes.TryAdd((ushort)StateChange.LivingEntityHorizontalInput, _horizontalInput);
            TriggerEntityUpdate();
            _horizontalInput = value;
        }
    }
    
    public float VerticalInput
    {
        get => _verticalInput;
        set
        {
            Changes.TryAdd((ushort)StateChange.LivingEntityVerticalInput, _verticalInput);
            TriggerEntityUpdate();
            _verticalInput = value;
        }
    }

    protected LivingEntityState(ushort id, EntityType type) : base(id, type)
    {
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

    protected override void SerializeChangeToChangePacket(Packet changePacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.LivingEntityHorizontalInput:
                changePacket.Write(HorizontalInput);
                break;
            case StateChange.LivingEntityVerticalInput:
                changePacket.Write(VerticalInput);
                break;
            case StateChange.LivingEntityJumpInput:
                changePacket.Write(JumpInput);
                break;
            default:
                base.SerializeChangeToChangePacket(changePacket, change);
                break;
        }
    }

    protected override void SerializeChangeToRevertPacket(Packet revertPacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.LivingEntityHorizontalInput or StateChange.LivingEntityVerticalInput:
                revertPacket.Write((float)Changes[change]);
                break;
            case StateChange.LivingEntityJumpInput:
                revertPacket.Write((bool)Changes[change]);
                break;
            default:
                base.SerializeChangeToRevertPacket(revertPacket, change);
                break;
        }
    }

    protected override void DeserializeChange(Packet packet, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.LivingEntityJumpInput:
                packet.Read(out _jumpInput);
                break;
            case StateChange.LivingEntityHorizontalInput:
                packet.Read(out _horizontalInput);
                break;
            case StateChange.LivingEntityVerticalInput:
                packet.Read(out _verticalInput);
                break;
            default:
                base.DeserializeChange(packet, change);
                break;
        }
    }
}