using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.States.World;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.World;

public class ChunkRenderer
{
    private readonly ChunkTessellator _tessellator = new();
    private readonly ChunkState _state;
    private HashSet<ushort> _dirtyVertexes = new();
    private readonly List<uint>[] _triangles = { new(), new(), new(), new(), new(), new() };
    
    public ChunkRenderer(ChunkState state)
    {
        _state = state;
        _tessellator.Position = state.ChunkPosition;
        _dirtyVertexes.EnsureCapacity(65536);
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
        result <<= 1;
        result |= world.GetBrightnessAt(pos - Vector3i.UnitY);
        result <<= 1;
        result |= world.GetBrightnessAt(pos + Vector3i.UnitX);
        result <<= 1;
        result |= world.GetBrightnessAt(pos - Vector3i.UnitX);
        result <<= 1;
        result |= world.GetBrightnessAt(pos + Vector3i.UnitZ);
        result <<= 1;
        result |= world.GetBrightnessAt(pos - Vector3i.UnitZ);
        return result;
    }

    public void UpdateRendererChanges(Packet packet)
    {
        packet.Read(out ushort changesCount);
        for (var i = 0; i < changesCount; i++)
        {
            packet.Read(out ushort index);
            _dirtyVertexes.Add(index);
        }
    }

    private void BuildTriangles()
    {
        _triangles[0].Clear();
        _triangles[1].Clear();
        _triangles[2].Clear();
        _triangles[3].Clear();
        _triangles[4].Clear();
        _triangles[5].Clear();
        for (ushort i = 0; i < EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth; i++)
            if (_state.GetBlockAt(i) != BlockType.Air)
                for (var j = 0; j < 6; j++)
                    if (ShouldDrawCubeFace(i, (BlockFaces)j))
                        _triangles[j].Add(i);
        _tessellator.SetTriangles(_triangles[0].ToArray(), _triangles[1].ToArray(), _triangles[2].ToArray(), _triangles[3].ToArray(), _triangles[4].ToArray(), _triangles[5].ToArray());
    }

    private bool ShouldDrawCubeFace(ushort index, BlockFaces face)
    {
        var world = MinecraftLibrary.Engine.World.GetInstance()!;
        var pos = EngineDefaults.GetVectorFromIndex(index) + _state.ChunkPosition;
        return face switch
        {
            BlockFaces.Bottom => !world.GetBlockAt(pos - Vector3i.UnitY).IsSolid(),
            BlockFaces.Top => !world.GetBlockAt(pos + Vector3i.UnitY).IsSolid(),
            BlockFaces.West => !world.GetBlockAt(pos - Vector3i.UnitX).IsSolid(),
            BlockFaces.East => !world.GetBlockAt(pos + Vector3i.UnitX).IsSolid(),
            BlockFaces.North => !world.GetBlockAt(pos + Vector3i.UnitZ).IsSolid(),
            BlockFaces.South => !world.GetBlockAt(pos - Vector3i.UnitZ).IsSolid(),
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }

    public void UpdateRenderer()
    {
        _tessellator.BeginUpdateVertex();
        foreach (var vertex in _dirtyVertexes) _tessellator.SetVertex(vertex, _state.GetBlockAt(vertex), BuildLightByte(vertex));
        _tessellator.EndUpdateVertex();
        BuildTriangles();
        _dirtyVertexes.Clear();
    }

    public override int GetHashCode()
    {
        return _tessellator.Position.GetHashCode();
    }

    public void Render()
    {
        _tessellator.Draw();
    }
}