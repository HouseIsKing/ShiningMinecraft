using MinecraftLibrary.Input;
using MinecraftLibrary.Network;

namespace MinecraftLibrary.Engine.States.Entities;

public sealed class PlayerState : LivingEntityState<PlayerState>
{
    private readonly (bool, object)[] _changes = new (bool, object)[Enum.GetValues<EPlayerChange>().Length];
    private byte _changesCount;
    private float _pitch;
    private bool _mode;
    private BlockType _currentSelectedBlock = BlockType.Cobblestone;
    public PlayerInput PlayerInput { get; } = new();

    public float Pitch
    {
        get => _pitch;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EPlayerChange.Pitch].Item1)
            {
                _changes[(byte)EPlayerChange.Pitch].Item1 = true;
                _changes[(byte)EPlayerChange.Pitch].Item2 = _pitch;
                _changesCount++;
            }

            _pitch = value;
        }
    }

    public bool Mode
    {
        get => _mode;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EPlayerChange.Mode].Item1)
            {
                _changes[(byte)EPlayerChange.Mode].Item1 = true;
                _changes[(byte)EPlayerChange.Mode].Item2 = _mode;
                _changesCount++;
            }

            _mode = value;
        }
    }

    public BlockType CurrentSelectedBlock
    {
        get => _currentSelectedBlock;
        set
        {
            IsDirty = true;
            if (!_changes[(byte)EPlayerChange.CurrentSelectedBlock].Item1)
            {
                _changes[(byte)EPlayerChange.CurrentSelectedBlock].Item1 = true;
                _changes[(byte)EPlayerChange.CurrentSelectedBlock].Item2 = _currentSelectedBlock;
                _changesCount++;
            }

            _currentSelectedBlock = value;
        }
    }

    public PlayerState(ushort id) : base(id, EntityType.Player)
    {
        for (var i = 0; i < _changes.Length; i++) _changes[i] = (false, id);
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

    public override void SerializeChangesToChangePacket(Packet changePacket)
    {
        base.SerializeChangesToChangePacket(changePacket);
        changePacket.Write(_changesCount);
        for (byte i = 0; i < _changes.Length; i++)
            if (_changes[i].Item1)
            {
                changePacket.Write(i);
                SerializeChangeToChangePacket(changePacket, i);
            }
    }

    private void SerializeChangeToChangePacket(Packet changePacket, byte change)
    {
        switch ((EPlayerChange)change)
        {
            case EPlayerChange.Pitch:
                changePacket.Write(Pitch);
                break;
            case EPlayerChange.Mode:
                changePacket.Write(Mode);
                break;
            case EPlayerChange.CurrentSelectedBlock:
                changePacket.Write((byte)CurrentSelectedBlock);
                break;
            case EPlayerChange.Input:
                PlayerInput.Serialize(changePacket);
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
            }
    }

    private void SerializeChangeToRevertPacket(Packet revertPacket, byte change)
    {
        switch ((EPlayerChange)change)
        {
            case EPlayerChange.Pitch:
                revertPacket.Write((float)_changes[change].Item2);
                break;
            case EPlayerChange.Mode:
                revertPacket.Write((bool)_changes[change].Item2);
                break;
            case EPlayerChange.CurrentSelectedBlock:
                revertPacket.Write((byte)_changes[change].Item2);
                break;
            case EPlayerChange.Input:
                ((PlayerInput)_changes[change].Item2).Serialize(revertPacket);
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
        switch ((EPlayerChange)change)
        {
            case EPlayerChange.Mode:
                packet.Read(out _mode);
                break;
            case EPlayerChange.Pitch:
                packet.Read(out _pitch);
                break;
            case EPlayerChange.CurrentSelectedBlock:
                packet.Read(out byte blockType);
                _currentSelectedBlock = (BlockType)blockType;
                break;
            case EPlayerChange.Input:
                PlayerInput.Deserialize(packet);
                break;
        }
    }

    public override void FinalizeChanges()
    {
        base.FinalizeChanges();
        _changesCount = 0;
        for (var i = 0; i < _changes.Length; i++) _changes[i].Item1 = false;
    }

    public void ApplyClientInput(ClientInput clientInput)
    {
        IsDirty = true;
        if (!_changes[(byte)EPlayerChange.Input].Item1)
        {
            _changes[(byte)EPlayerChange.Input].Item1 = true;
            _changes[(byte)EPlayerChange.Input].Item2 = PlayerInput;
            _changesCount++;
        }

        PlayerInput.ApplyClientInput(clientInput);
    }
}