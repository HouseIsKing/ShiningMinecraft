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
    private readonly List<uint>[] _triangles = { new(), new(), new() };

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
        _triangles[0].Clear();
        _triangles[1].Clear();
        _triangles[2].Clear();
        for (ushort i = 0; i < EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth; i++)
            if (_state.GetBlockAt(i) != BlockType.Air)
                for (var j = 0; j < 3; j++)
                    if (ShouldDrawCubeFace(i, (BlockFaces)(j * 2)) || ShouldDrawCubeFace(i, (BlockFaces)(j * 2 + 1)))
                        _triangles[j].Add(i);
        tessellator.SetTriangles(ChunkId, _triangles[0].ToArray(), _triangles[1].ToArray(), _triangles[2].ToArray());
    }

    private bool ShouldDrawCubeFace(ushort index, BlockFaces face)
    {
        var pos = EngineDefaults.GetVectorFromIndex(index) + _state.ChunkPosition;
        var world = MinecraftLibrary.Engine.World.GetInstance()!;
        return face switch
        {
            BlockFaces.Bottom => !world.GetBlockAt(pos - Vector3i.UnitY).IsSolid(),
            BlockFaces.Top => !world.GetBlockAt(pos + Vector3i.UnitY).IsSolid(),
            BlockFaces.West => !world.GetBlockAt(pos - Vector3i.UnitX).IsSolid(),
            BlockFaces.East => !world.GetBlockAt(pos + Vector3i.UnitX).IsSolid(),
            BlockFaces.North => !world.GetBlockAt(pos + Vector3i.UnitZ).IsSolid(),
            BlockFaces.South => !world.GetBlockAt(pos - Vector3i.UnitZ).IsSolid(),
            _ => false
        };
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