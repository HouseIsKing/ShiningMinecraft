using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.World;

public struct ChunkState : IState
{
    public Vector3i ChunkPosition;
    public BlockType[] Blocks = new BlockType[EngineDefaults.ChunkHeight * EngineDefaults.ChunkWidth * EngineDefaults.ChunkDepth];

    public ChunkState(Vector3i pos)
    {
        ChunkPosition = pos;
    }

    public readonly void Serialize(Packet packet)
    {
        packet.Write(ChunkPosition);
        foreach (var block in Blocks) packet.Write((byte)block);
    }

    public void Deserialize(Packet packet)
    {
        packet.Read(out ChunkPosition);
        for (var i = 0; i < Blocks.Length; i++)
        {
            packet.Read(out byte item);
            Blocks[i] = (BlockType)item;
        }
    }
}