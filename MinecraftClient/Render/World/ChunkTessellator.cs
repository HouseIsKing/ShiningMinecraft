using System.Runtime.InteropServices;
using MinecraftClient.Render.Shaders;
using MinecraftClient.Render.World.Block;
using MinecraftLibrary.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.World;

internal sealed class ChunkTessellator : Tessellator
{
    private readonly IntPtr _vboPointer;
    private readonly IntPtr _eboPointer;
    private readonly IntPtr _drawElementsIndirectCommandsPointer;
    private readonly uint _chunksTransformsBuffer;
    private readonly IntPtr _chunksTransformsPointer;
    private static readonly byte[] VertexHelper = new byte[4];
    private readonly ushort[] _drawCommandCount = new ushort[Enum.GetValues<BlockType>().Length];
    private readonly int _drawElementsSize;
    
    private const int VboBufferSize = EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * sizeof(uint);
    private const int EboBufferSize = EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * sizeof(uint);

    ~ChunkTessellator()
    {
        GL.UnmapNamedBuffer(DrawElementsIndirectCommandsBuffer);
        GL.UnmapNamedBuffer(_chunksTransformsBuffer);
        GL.DeleteBuffer(_chunksTransformsBuffer);
    }

    public ChunkTessellator(ushort chunksCount)
    {
        GL.EnableVertexArrayAttrib(Vao, 0);
        GL.VertexArrayAttribIFormat(Vao, 0, 1, VertexAttribIntegerType.UnsignedInt, 0);
        GL.VertexArrayAttribBinding(Vao, 0, 0);
        GL.CreateBuffers(1, out _chunksTransformsBuffer);
        var helper = chunksCount * Marshal.SizeOf<Matrix4>();
        GL.NamedBufferStorage(_chunksTransformsBuffer,
            helper, new byte[helper],
            BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        _chunksTransformsPointer = GL.MapNamedBufferRange(_chunksTransformsBuffer, 0, helper,
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, _chunksTransformsBuffer);
        
        _drawElementsSize = chunksCount * Marshal.SizeOf<GlDrawElementsIndirectCommand>();
        GL.NamedBufferStorage(DrawElementsIndirectCommandsBuffer, _drawElementsSize * Enum.GetValues<DrawType>().Length, IntPtr.Zero, 
            BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        _drawElementsIndirectCommandsPointer = GL.MapNamedBufferRange(DrawElementsIndirectCommandsBuffer, 0, _drawElementsSize * Enum.GetValues<DrawType>().Length,
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
        
        GL.BindVertexArray(Vao);
        GL.NamedBufferStorage(Vbo, chunksCount * VboBufferSize, IntPtr.Zero,
            BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        _vboPointer = GL.MapNamedBufferRange(Vbo, 0, chunksCount * VboBufferSize,
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
        GL.VertexArrayVertexBuffer(Vao, 0, Vbo, IntPtr.Zero, sizeof(uint));
        
        GL.NamedBufferStorage(Ebo, chunksCount * EboBufferSize, IntPtr.Zero,
            BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        _eboPointer = GL.MapNamedBufferRange(Ebo, 0, chunksCount * EboBufferSize,
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
        GL.VertexArrayElementBuffer(Vao, Ebo);
    }

    internal void SetTriangles(ushort chunkId, uint[] triangles)
    {
        var offset = chunkId * EboBufferSize;
        Marshal.Copy((int[])(object)triangles, 0, _eboPointer + offset, triangles.Length);
    }

    internal static void ChangeDrawType(DrawType type)
    {
        switch (type)
        {
            case DrawType.Normal:
                Shader.NormalBlockShader.Use();
                break;
            case DrawType.Cross:
                Shader.CrossBlockShader.Use();
                break;
        }
    }

    internal override void Draw()
    {
        foreach (var type in Enum.GetValues<DrawType>())
        {
            ChangeDrawType(type);
            GL.MultiDrawElementsIndirect(PrimitiveType.Points, DrawElementsType.UnsignedInt, _drawElementsSize * (byte)type, _drawCommandCount[(byte)type], Marshal.SizeOf<GlDrawElementsIndirectCommand>());
            _drawCommandCount[(byte)type] = 0;
        }
    }

    internal void SetVertex(ushort chunkId, ushort index, BlockType blockType, byte light)
    {
        var finalIndex = chunkId * VboBufferSize + index * sizeof(uint);
        var constructedVertex = (uint)blockType << 16;
        constructedVertex |= (uint)light << 10;
        VertexHelper[3] = (byte)(constructedVertex >> 24);
        VertexHelper[2] = (byte)((constructedVertex >> 16) & 0xFF);
        VertexHelper[1] = (byte)((constructedVertex >> 8) & 0xFF);
        VertexHelper[0] = (byte)(constructedVertex & 0xFF);
        Marshal.Copy(VertexHelper, 0, _vboPointer + finalIndex, sizeof(uint));
    }
    
    public void SetChunkTransform(ushort chunkId, Matrix4 transform)
    {
        Marshal.Copy(EngineDefaults.GetBytes(transform).ToArray(), 0, _chunksTransformsPointer + chunkId * Marshal.SizeOf<Matrix4>(), Marshal.SizeOf<Matrix4>());
    }

    internal void AppendDrawCommand(DrawType type, byte[] command)
    {
        Marshal.Copy(command, 0,
            _drawElementsIndirectCommandsPointer + (byte)type * _drawElementsSize + _drawCommandCount[(byte)type] * Marshal.SizeOf<GlDrawElementsIndirectCommand>(),
            Marshal.SizeOf<GlDrawElementsIndirectCommand>());
        _drawCommandCount[(byte)type]++;
    }
}