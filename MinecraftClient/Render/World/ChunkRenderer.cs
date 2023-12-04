using MinecraftClient.Render.Entities.Player;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.States.World;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.World;

public sealed class ChunkRenderer
{
    public ushort ChunkId { get; }
    private readonly ChunkState _state;

    private readonly HashSet<ushort> _dirtyVertexes =
        new(EngineDefaults.ChunkHeight * EngineDefaults.ChunkWidth * EngineDefaults.ChunkDepth);
    private readonly List<uint> _triangles = new();

    private Vector3 Position { get; }
    private static Vector3 Rotation => Vector3.Zero;
    private static Vector3 Scale => Vector3.One;

    public Matrix4 ModelMatrix => Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
    
    public ChunkRenderer(ChunkState state, ushort id)
    {
        ChunkId = id;
        _state = state;
        Position = _state.ChunkPosition;
        for (ushort i = 0; i < EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth; i++) _dirtyVertexes.Add(i);
    }

    public bool IsDirty()
    {
        return _dirtyVertexes.Count > 0;
    }

    private byte BuildLightByte(ushort index)
    {
        var pos = EngineDefaults.GetVectorFromIndex(index) + _state.ChunkPosition;
        var world = MinecraftLibrary.Engine.World.GetInstance()!;
        var result = world.GetBrightnessAt(pos + Vector3i.UnitY);
        result |= (byte)(world.GetBrightnessAt(pos + Vector3i.UnitX) << 2);
        result |= (byte)(world.GetBrightnessAt(pos - Vector3i.UnitX) << 3);
        result |= (byte)(world.GetBrightnessAt(pos + Vector3i.UnitZ) << 4);
        result |= (byte)(world.GetBrightnessAt(pos - Vector3i.UnitZ) << 5);
        return result;
    }

    public void UpdateRendererChanges(Packet packet)
    {
        packet.Read(out ushort changesCount);
        for (var i = 0; i < changesCount; i++)
        {
            packet.Read(out ushort index);
            _dirtyVertexes.Add(index);
            packet.Read(out byte _);
        }
    }

    private void BuildTriangles(ChunkTessellator tessellator)
    {
        _triangles.Clear();
        for (ushort i = 0; i < EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth; i++)
            if (_state.GetBlockAt(i) != BlockType.Air && ShouldDrawCube(i)) 
                _triangles.Add(i);
        tessellator.SetTriangles(ChunkId, _triangles.ToArray());
    }

    private bool ShouldDrawCube(ushort index)
    {
        var pos = EngineDefaults.GetVectorFromIndex(index) + _state.ChunkPosition;
        var world = MinecraftLibrary.Engine.World.GetInstance()!;
        return !world.GetBlockAt(pos + Vector3i.UnitY).IsSolid() ||
               !world.GetBlockAt(pos - Vector3i.UnitY).IsSolid() ||
               !world.GetBlockAt(pos + Vector3i.UnitX).IsSolid() ||
               !world.GetBlockAt(pos - Vector3i.UnitX).IsSolid() ||
               !world.GetBlockAt(pos + Vector3i.UnitZ).IsSolid() ||
               !world.GetBlockAt(pos - Vector3i.UnitZ).IsSolid();
    }

    public void UpdateRenderer(ChunkTessellator tessellator)
    {
        foreach (var vertex in _dirtyVertexes) tessellator.SetVertex(ChunkId, vertex, _state.GetBlockAt(vertex), BuildLightByte(vertex));
        BuildTriangles(tessellator);
        _dirtyVertexes.Clear();
    }

    public override int GetHashCode()
    {
        return _state.ChunkPosition.GetHashCode();
    }
}