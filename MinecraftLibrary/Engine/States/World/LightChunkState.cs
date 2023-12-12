using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.World;

public sealed class LightChunkState(Vector2i pos) : State<LightChunkState>
{
    public Vector2i LightPosition { get; } = pos;
    public byte LightLevel
    {
        get => _lightLevel;
        set
        {
            IsDirty = true;
            _changes = LightLevel;
            _lightLevel = value;
        }
    }
    private byte _lightLevel;
    private byte _changes;

    public override void Serialize(Packet packet)
    {
        packet.Write(LightPosition);
        packet.Write(LightLevel);
    }

    public override void Deserialize(Packet packet)
    {
        packet.Read(out _lightLevel);
    }

    public override void SerializeChangesToChangePacket(Packet changePacket)
    {
        base.SerializeChangesToChangePacket(changePacket);
        changePacket.Write(_lightLevel);
    }

    public override void SerializeChangesToRevertPacket(Packet revertPacket)
    {
        base.SerializeChangesToRevertPacket(revertPacket);
        revertPacket.Write(_changes);
    }

    public override void DeserializeChanges(Packet changePacket)
    {
        changePacket.Read(out _lightLevel);
    }
}