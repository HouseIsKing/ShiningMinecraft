using System.Reflection.Metadata;
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
    private GlDrawElementsIndirectCommand _command;
    private byte[] drawCommand = new byte[20];
    private Box3 _boundingBox;

    private Vector3 Position { get; }
    private static Vector3 Rotation => Vector3.Zero;
    private static Vector3 Scale => Vector3.One;

    public Matrix4 ModelMatrix => Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
    
    public ChunkRenderer(ChunkState state, ushort id)
    {
        ChunkId = id;
        _state = state;
        WorldRenderer renderer = WorldRenderer.Instance;
        Vector3i pos = new Vector3i(_state.X * EngineDefaults.ChunkWidth, _state.Y * EngineDefaults.ChunkHeight, _state.Z * EngineDefaults.ChunkDepth) + renderer.BaseVector;
        _boundingBox = new Box3(pos, pos + new Vector3i(EngineDefaults.ChunkWidth, EngineDefaults.ChunkHeight, EngineDefaults.ChunkDepth));
        Position = pos;
        for (ushort i = 0; i < EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth; i++) _dirtyVertexes.Add(i);
        _command.instanceCount = 1;
        _command.baseInstance = 0;
        _command.baseVertex = (uint)(ChunkId * EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight *
                                     EngineDefaults.ChunkDepth);
        _command.count = 0;
        _command.firstIndex = (uint)(ChunkId * EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth);
    }

    public bool IsDirty()
    {
        return _dirtyVertexes.Count > 0;
    }

    private byte BuildLightByte(ushort index)
    {
        var world = MinecraftLibrary.Engine.World.Instance;
        var pos = EngineDefaults.GetVectorFromIndex(index) + new Vector3i(_state.X * EngineDefaults.ChunkWidth,
            _state.Y * EngineDefaults.ChunkHeight, _state.Z * EngineDefaults.ChunkDepth) + world.GetBaseVector();
        var result = world.GetBrightnessAt(pos + Vector3i.UnitY);
        result |= (byte)(world.GetBrightnessAt(pos + Vector3i.UnitX) << 2);
        result |= (byte)(world.GetBrightnessAt(pos - Vector3i.UnitX) << 3);
        result |= (byte)(world.GetBrightnessAt(pos + Vector3i.UnitZ) << 4);
        result |= (byte)(world.GetBrightnessAt(pos - Vector3i.UnitZ) << 5);
        return result;
    }
    
    public void NotifyBlockChange(ushort index)
    {
        _dirtyVertexes.Add(index);
    }

    public void UpdateRendererChanges(Packet packet)
    {
        packet.Read(out ushort changesCount);
        for (var i = 0; i < changesCount; i++)
        {
            packet.Read(out ushort index);
            packet.Read(out byte _);
            NotifyBlockChange(index);
            WorldRenderer renderer = WorldRenderer.Instance;
            if (MinecraftLibrary.Engine.Blocks.Block.GetBlock(_state.GetBlockAt(index)).IsSolid()) continue;
            var helper = EngineDefaults.GetVectorFromIndex(index) + new Vector3i(_state.X * EngineDefaults.ChunkWidth,
                _state.Y * EngineDefaults.ChunkHeight, _state.Z * EngineDefaults.ChunkDepth) + renderer.BaseVector;
            var pos = helper + Vector3i.UnitY;
            if (!renderer.IsOutOfBounds(pos)) renderer.GetChunkRendererAt(pos).NotifyBlockChange(EngineDefaults.GetIndexFromVector(pos));
            pos = helper - Vector3i.UnitY;
            if (!renderer.IsOutOfBounds(pos)) renderer.GetChunkRendererAt(pos).NotifyBlockChange(EngineDefaults.GetIndexFromVector(pos));
            pos = helper + Vector3i.UnitX;
            if (!renderer.IsOutOfBounds(pos)) renderer.GetChunkRendererAt(pos).NotifyBlockChange(EngineDefaults.GetIndexFromVector(pos));
            pos = helper - Vector3i.UnitX;
            if (!renderer.IsOutOfBounds(pos)) renderer.GetChunkRendererAt(pos).NotifyBlockChange(EngineDefaults.GetIndexFromVector(pos));
            pos = helper + Vector3i.UnitZ;
            if (!renderer.IsOutOfBounds(pos)) renderer.GetChunkRendererAt(pos).NotifyBlockChange(EngineDefaults.GetIndexFromVector(pos));
            pos = helper - Vector3i.UnitZ;
            if (!renderer.IsOutOfBounds(pos)) renderer.GetChunkRendererAt(pos).NotifyBlockChange(EngineDefaults.GetIndexFromVector(pos));
        }
    }

    private void BuildTriangles(ChunkTessellator tessellator)
    {
        _triangles.Clear();
        for (ushort i = 0; i < EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth; i++)
            if (_state.GetBlockAt(i) != BlockType.Air && ShouldDrawCube(i)) 
                _triangles.Add(i);
        _command.count = (uint)_triangles.Count;
        tessellator.SetTriangles(ChunkId, _triangles.ToArray());
    }

    private bool ShouldDrawCube(ushort index)
    {
        var world = MinecraftLibrary.Engine.World.Instance;
        var pos = EngineDefaults.GetVectorFromIndex(index) + new Vector3i(_state.X * EngineDefaults.ChunkWidth,
            _state.Y * EngineDefaults.ChunkHeight, _state.Z * EngineDefaults.ChunkDepth) + world.GetBaseVector();
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
        drawCommand = EngineDefaults.GetBytes(_command).ToArray();
    }

    public void LightUpdateColumn(Vector2i pos)
    {
        var index = EngineDefaults.GetIndexFromVector(new Vector3i(pos.X, 0, pos.Y));
        for (var i = 0; i < EngineDefaults.ChunkHeight; i++)
        {
            _dirtyVertexes.Add(index);
            index += EngineDefaults.ChunkDepth;
        }
    }

    public byte[] GetDrawCommand()
    {
        return drawCommand;
    }

    public Box3 GetBoundingBox()
    {
        return _boundingBox;
    }
}