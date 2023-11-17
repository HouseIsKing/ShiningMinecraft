using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States.Entities;

public class BlockParticleEntityState : EntityState<BlockParticleEntityState>
{
    private BlockType _blockParticleType = BlockType.Air;
    private byte _lifeTime = 0;
    private byte _maxLifeTime = 0;
    
    public byte LifeTime
    {
        get => _lifeTime;
        set
        {
            Changes.TryAdd((ushort)StateChange.BlockParticleLifeTime, _lifeTime);
            TriggerEntityUpdate();
            _lifeTime = value;
        }
    }
    
    public byte MaxLifeTime
    {
        get => _maxLifeTime;
        set
        {
            Changes.TryAdd((ushort)StateChange.BlockParticleMaxLifeTime, _maxLifeTime);
            TriggerEntityUpdate();
            _maxLifeTime = value;
        }
    }
    
    public BlockType BlockParticleType
    {
        get => _blockParticleType;
        set
        {
            Changes.TryAdd((ushort)StateChange.BlockParticleType, _blockParticleType);
            TriggerEntityUpdate();
            _blockParticleType = value;
        }
    }

    public BlockParticleEntityState(ushort id) : base(id, EntityType.BlockBreakParticle)
    {
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

    protected override void SerializeChangeToChangePacket(Packet changePacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.BlockParticleLifeTime:
                changePacket.Write(LifeTime);
                break;
            case StateChange.BlockParticleMaxLifeTime:
                changePacket.Write(MaxLifeTime);
                break;
            case StateChange.BlockParticleType:
                changePacket.Write((byte)BlockParticleType);
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
            case StateChange.BlockParticleLifeTime:
                revertPacket.Write((byte)Changes[change]);
                break;
            case StateChange.BlockParticleMaxLifeTime:
                revertPacket.Write((byte)Changes[change]);
                break;
            case StateChange.BlockParticleType:
                revertPacket.Write((byte)Changes[change]);
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
            case StateChange.BlockParticleType:
                packet.Read(out byte helper);
                _blockParticleType = (BlockType)helper;
                break;
            case StateChange.BlockParticleLifeTime:
                packet.Read(out _lifeTime);
                break;
            case StateChange.BlockParticleMaxLifeTime:
                packet.Read(out _maxLifeTime);
                break;
            default:
                base.DeserializeChange(packet, change);
                break;
        }
    }
}