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
        
    }

    public void Render()
    {
        
    }
}