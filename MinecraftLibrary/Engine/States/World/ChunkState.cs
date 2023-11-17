using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.World;

public sealed class ChunkState : State<ChunkState>
{
    public event EngineDefaults.ChunkUpdateHandler? OnChunkUpdate;
    public Vector3i ChunkPosition { get; }
    private BlockType[] Blocks { get; } = new BlockType[EngineDefaults.ChunkHeight * EngineDefaults.ChunkWidth * EngineDefaults.ChunkDepth];

    public ChunkState(Vector3i pos)
    {
        ChunkPosition = pos;
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

    protected override void SerializeChangeToChangePacket(Packet changePacket, ushort change)
    {
        changePacket.Write((byte)Blocks[change]);
    }

    protected override void SerializeChangeToRevertPacket(Packet revertPacket, ushort change)
    {
        revertPacket.Write((byte)Changes[change]);
    }

    protected override void DeserializeChange(Packet changePacket, ushort change)
    {
        changePacket.Read(out byte blockType);
        Blocks[change] = (BlockType)blockType;
    }

    public void SetBlockAt(ushort index, BlockType blockType)
    {
        Changes.TryAdd(index, Blocks[index]);
        OnChunkUpdate?.Invoke(ChunkPosition, index, blockType);
        Blocks[index] = blockType;
    }

    public BlockType GetBlockAt(ushort index)
    {
        return Blocks[index];
    }
}