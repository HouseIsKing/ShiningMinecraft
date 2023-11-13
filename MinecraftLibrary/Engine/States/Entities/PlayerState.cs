using MinecraftLibrary.Input;
using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States.Entities;

public class PlayerState : LivingEntityState<PlayerState>
{
    private float _pitch = 0.0F;
    private bool _mode = false;
    private BlockType _currentSelectedBlock = BlockType.Cobblestone;
    public PlayerInput PlayerInput { get; } = new();
    
    public float Pitch
    {
        get => _pitch;
        set
        {
            Changes.TryAdd((ushort)StateChange.PlayerPitch, _pitch);
            TriggerEntityUpdate();
            _pitch = value;
        }
    }
    
    public bool Mode
    {
        get => _mode;
        set
        {
            Changes.TryAdd((ushort)StateChange.PlayerMode, _mode);
            TriggerEntityUpdate();
            _mode = value;
        }
    }
    
    public BlockType CurrentSelectedBlock
    {
        get => _currentSelectedBlock;
        set
        {
            Changes.TryAdd((ushort)StateChange.PlayerCurrentSelectedBlock, _currentSelectedBlock);
            TriggerEntityUpdate();
            _currentSelectedBlock = value;
        }
    }
    
    public PlayerState(ushort id) : base(id, EntityType.Player)
    {
    }

    public override void Serialize(Packet packet)
    {
        base.Serialize(packet);
        packet.Write(Pitch);
        packet.Write(Mode);
        packet.Write((byte)CurrentSelectedBlock);
        PlayerInput.Serialize(packet);
    }

    public override void Deserialize(Packet packet)
    {
        base.Deserialize(packet);
        packet.Read(out _pitch);
        packet.Read(out _mode);
        packet.Read(out byte blockType);
        _currentSelectedBlock = (BlockType)blockType;
        PlayerInput.Deserialize(packet);
    }

    protected override void SerializeChangeToChangePacket(Packet changePacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.PlayerPitch:
                changePacket.Write(Pitch);
                break;
            case StateChange.PlayerMode:
                changePacket.Write(Mode);
                break;
            case StateChange.PlayerCurrentSelectedBlock:
                changePacket.Write((byte)CurrentSelectedBlock);
                break;
            case StateChange.PlayerInput:
                PlayerInput.Serialize(changePacket);
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
            case StateChange.PlayerPitch:
                revertPacket.Write((float)Changes[change]);
                break;
            case StateChange.PlayerMode:
                revertPacket.Write((bool)Changes[change]);
                break;
            case StateChange.PlayerCurrentSelectedBlock:
                revertPacket.Write((byte)CurrentSelectedBlock);
                break;
            case StateChange.PlayerInput:
                ((PlayerInput)Changes[change]).Serialize(revertPacket);
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
            case StateChange.PlayerMode:
                packet.Read(out _mode);
                break;
            case StateChange.PlayerPitch:
                packet.Read(out _pitch);
                break;
            case StateChange.PlayerCurrentSelectedBlock:
                packet.Read(out byte blockType);
                _currentSelectedBlock = (BlockType)blockType;
                break;
            case StateChange.PlayerInput:
                PlayerInput.Deserialize(packet);
                break;
            default:
                base.DeserializeChange(packet, change);
                break;
        }
    }
    
    public void ApplyClientInput(ClientInput clientInput)
    {
        Changes.TryAdd((ushort)StateChange.PlayerInput, PlayerInput);
        TriggerEntityUpdate();
        PlayerInput.ApplyClientInput(clientInput);
    }
}