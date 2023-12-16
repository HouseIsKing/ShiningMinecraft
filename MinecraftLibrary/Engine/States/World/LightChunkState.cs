using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.World;

public sealed class LightChunkState(ushort x, ushort z) : State<LightChunkState>
{
    public ushort X { get; } = x;
    public ushort Z { get; } = z;
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
        packet.Write(X);
        packet.Write(Z);
        packet.Write(LightLevel);
    }

    public override void Deserialize(Packet packet)
    {
        packet.Read(out _lightLevel);
    }

    public override void SerializeChangesToChangePacket(Packet changePacket)
    {
        if (!IsDirty) return;
        changePacket.Write(X);
        changePacket.Write(Z);
        base.SerializeChangesToChangePacket(changePacket);
        changePacket.Write(_lightLevel);
    }

    public override void SerializeChangesToRevertPacket(Packet revertPacket)
    {
        if (!IsDirty) return;
        revertPacket.Write(X);
        revertPacket.Write(Z);
        base.SerializeChangesToRevertPacket(revertPacket);
        revertPacket.Write(_changes);
    }

    public override void DeserializeChanges(Packet changePacket)
    {
        changePacket.Read(out _lightLevel);
        LightLevel = _lightLevel;
    }
}