using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.States.World;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.World;

public class ChunkRenderer
{
    public delegate void ChunkRenderDirty(ChunkRenderer renderer);
    
    public event ChunkRenderDirty? OnChunkRenderDirty;
    private readonly ChunkTessellator _tessellator = new();
    
    private bool _isDirty = true;
    private List<ushort> _triangles = new();
    
    public ChunkRenderer(ChunkState state)
    {
        _tessellator.Position = state.ChunkPosition;
        state.OnChunkUpdate += OnChunkUpdate;
    }

    private static ushort GetFaceIndexer0(BlockFaces face)
    {
        return face switch
        {
            BlockFaces.Bottom or BlockFaces.South or BlockFaces.West => 0,
            BlockFaces.Top => EngineDefaults.GetIndexFromVector(Vector3i.UnitY),
            BlockFaces.East => EngineDefaults.GetIndexFromVector(Vector3i.UnitX),
            BlockFaces.North => EngineDefaults.GetIndexFromVector(Vector3i.UnitZ),
            _ => throw new Exception("Invalid face")
        };
    }

    private static ushort GetFaceIndexer1(BlockFaces face)
    {
        return face switch
        {
            BlockFaces.South or BlockFaces.Bottom => EngineDefaults.GetIndexFromVector(Vector3i.UnitX),
            BlockFaces.North or BlockFaces.East => EngineDefaults.GetIndexFromVector(Vector3i.UnitX + Vector3i.UnitZ),
            BlockFaces.Top => EngineDefaults.GetIndexFromVector(Vector3i.UnitY + Vector3i.UnitX),
            BlockFaces.West => EngineDefaults.GetIndexFromVector(Vector3i.UnitZ),
            _ => throw new Exception("Invalid face")
        };
    }

    private static ushort GetFaceIndexer2(BlockFaces face)
    {
        return face switch
        {
            BlockFaces.South or BlockFaces.West => EngineDefaults.GetIndexFromVector(Vector3i.UnitY),
            BlockFaces.North or BlockFaces.Top => EngineDefaults.GetIndexFromVector(Vector3i.UnitZ + Vector3i.UnitY),
            BlockFaces.Bottom => EngineDefaults.GetIndexFromVector(Vector3i.UnitZ),
            BlockFaces.East => EngineDefaults.GetIndexFromVector(Vector3i.UnitX + Vector3i.UnitY),
            _ => throw new Exception("Invalid face")
        };
    }

    private static ushort GetFaceIndexer3(BlockFaces face)
    {
        return face switch
        {
            BlockFaces.North or BlockFaces.East or BlockFaces.Top => EngineDefaults.GetIndexFromVector(Vector3i.One),
            BlockFaces.South => EngineDefaults.GetIndexFromVector(Vector3i.UnitX + Vector3i.UnitY),
            BlockFaces.West => EngineDefaults.GetIndexFromVector(Vector3i.UnitZ + Vector3i.UnitY),
            BlockFaces.Bottom => EngineDefaults.GetIndexFromVector(Vector3i.UnitX + Vector3i.UnitZ),
            _ => throw new Exception("Invalid face")
        };
    }

    private static ushort GetFaceTriangleIndexer(byte triangle, BlockFaces face)
    {
        return triangle switch
        {
            0 => GetFaceIndexer0(face),
            1 => GetFaceIndexer1(face),
            2 => GetFaceIndexer2(face),
            3 => GetFaceIndexer3(face),
            _ => throw new Exception("Invalid triangle")
        };
    }

    private static Vector3i GetNormalFromFace(BlockFaces face)
    {
        return face switch
        {
            BlockFaces.Bottom => -Vector3i.UnitY,
            BlockFaces.Top => Vector3i.UnitY,
            BlockFaces.North => Vector3i.UnitZ,
            BlockFaces.South => -Vector3i.UnitZ,
            BlockFaces.East => Vector3i.UnitX,
            BlockFaces.West => -Vector3i.UnitX,
            _ => throw new Exception("Invalid face")
        };
    }

    private void OnChunkUpdate(Vector3i chunkPosition, ushort change, BlockType type)
    {
        //var vertices = new ChunkVertex[24];
        foreach (var face in Enum.GetValues<BlockFaces>())
            for (byte i = 0; i < 4; i++)
                _tessellator.SetVertex((ushort)(change + GetFaceTriangleIndexer(i, face)), (byte)((byte)face / 2),
                    WorldRenderer.BlockRenderers[(int)face].GenerateFaceVertex(face, i, Vector4.One,
                        MinecraftLibrary.Engine.World.GetInstance()!.GetBrightnessAt(chunkPosition +
                            EngineDefaults.GetVectorFromIndex(change))));

        OnChunkRenderDirty?.Invoke(this);
        _isDirty = true;
    }

    public override int GetHashCode()
    {
        return _tessellator.Position.GetHashCode();
    }

    public void ClearDirty()
    {
        var world = MinecraftLibrary.Engine.World.GetInstance()!;
        for (var x = 0; x < EngineDefaults.ChunkWidth; x++)
        for (var y = 0; y < EngineDefaults.ChunkHeight; y++)
        for (var z = 0; z < EngineDefaults.ChunkDepth; z++)
        {
            Vector3i pos = new(x, y, z);
            var block = world.GetBlockAt((Vector3i)_tessellator.Position + new Vector3i(x, y, z));
            if (block.Type == BlockType.Air) continue;

            foreach (var face in Enum.GetValues<BlockFaces>())
            {
                if (world.GetBlockAt(pos + GetNormalFromFace(face)).IsSolid()) continue;

                var index0 = (ushort)(EngineDefaults.GetIndexFromVector(pos) + GetFaceTriangleIndexer(0, face));
                var index1 = (ushort)(EngineDefaults.GetIndexFromVector(pos) + GetFaceTriangleIndexer(1, face));
                var index2 = (ushort)(EngineDefaults.GetIndexFromVector(pos) + GetFaceTriangleIndexer(2, face));
                var index3 = (ushort)(EngineDefaults.GetIndexFromVector(pos) + GetFaceTriangleIndexer(3, face));
                _triangles.Add(index0);
                _triangles.Add(index1);
                _triangles.Add(index2);
                _triangles.Add(index3);
                _triangles.Add(index2);
                _triangles.Add(index1);
            }
        }

        _tessellator.SetTriangles(_triangles.ToArray());
        _triangles.Clear();
    }

    public void Render()
    {
        
    }
}