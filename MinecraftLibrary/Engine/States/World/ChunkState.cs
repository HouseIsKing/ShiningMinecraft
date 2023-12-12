using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.World;

public sealed class ChunkState : State<ChunkState>
{
    public Vector3i ChunkPosition { get; }
    private BlockType[] Blocks { get; } = new BlockType[EngineDefaults.ChunkHeight * EngineDefaults.ChunkWidth * EngineDefaults.ChunkDepth];
    private readonly (bool, BlockType)[] _changes = new (bool, BlockType)[EngineDefaults.ChunkHeight * EngineDefaults.ChunkWidth * EngineDefaults.ChunkDepth];
    private ushort _changesCount;

    public ChunkState(Vector3i pos)
    {
        ChunkPosition = pos;
        for (var i = 0; i < _changes.Length; i++) _changes[i] = (false, BlockType.Air);
    }

    public override void Serialize(Packet packet)
    {
        packet.Write(ChunkPosition);
        foreach (var block in Blocks) packet.Write((byte)block);
    }

    public override void Deserialize(Packet packet)
    {
        for (var i = 0; i < Blocks.Length; i++)
        {
            packet.Read(out byte item);
            Blocks[i] = (BlockType)item;
        }
    }

    public override void SerializeChangesToChangePacket(Packet changePacket)
    {
        base.SerializeChangesToChangePacket(changePacket);
        changePacket.Write(_changesCount);
        for (ushort i = 0; i < _changes.Length; i++)
            if (_changes[i].Item1)
            {
                changePacket.Write(i);
                changePacket.Write((byte)Blocks[i]);
                _changes[i].Item1 = false;
            }
        _changesCount = 0;
    }

    public override void SerializeChangesToRevertPacket(Packet revertPacket)
    {
        base.SerializeChangesToRevertPacket(revertPacket);
        revertPacket.Write(_changesCount);
        for (ushort i = 0; i < _changes.Length; i++)
            if (_changes[i].Item1)
            {
                revertPacket.Write(i);
                revertPacket.Write((byte)_changes[i].Item2);
                _changes[i].Item1 = false;
            }

        _changesCount = 0;
    }

    public override void DeserializeChanges(Packet changePacket)
    {
        changePacket.Read(out ushort changeCount);
        for (var i = 0; i < changeCount; i++)
        {
            changePacket.Read(out ushort index);
            changePacket.Read(out byte blockType);
            Blocks[index] = (BlockType)blockType;
        }
    }

    public void SetBlockAt(ushort index, BlockType blockType)
    {
        IsDirty = true;
        if (!_changes[index].Item1)
        {
            _changes[index].Item1 = true;
            _changes[index].Item2 = Blocks[index];
            _changesCount++;
        }

        Blocks[index] = blockType;
    }

    public BlockType GetBlockAt(ushort index)
    {
        return Blocks[index];
    }

    public override void DiscardChanges()
    {
        base.DiscardChanges();
        _changesCount = 0;
        for (ushort i = 0; i < _changes.Length; i++)
            _changes[i].Item1 = false;
    }
}