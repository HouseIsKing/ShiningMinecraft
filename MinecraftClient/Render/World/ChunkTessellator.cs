using System.Runtime.InteropServices;
using MinecraftLibrary.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.World;

public sealed class ChunkTessellator : Tessellator
{
    private readonly IntPtr _vboPointer;
    private readonly IntPtr _eboPointer;
    private readonly uint _drawElementsIndirectCommandsBuffer;
    private readonly IntPtr _drawElementsIndirectCommandsPointer;
    private readonly uint _chunksTransformsBuffer;
    private readonly IntPtr _chunksTransformsPointer;
    private readonly ushort _chunksCount;
    private static readonly uint Vao;
    
    private const int VboBufferSize = EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * sizeof(uint);
    private const int EboBufferSize = EngineDefaults.ChunkWidth / 2 * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * 3 * sizeof(uint);

    static ChunkTessellator()
    {
        GL.CreateVertexArrays(1, out Vao);
        GL.EnableVertexArrayAttrib(Vao, 0);
        GL.VertexArrayAttribIFormat(Vao, 0, 1, VertexAttribIntegerType.UnsignedInt, 0);
        GL.VertexArrayAttribBinding(Vao, 0, 0);
        GL.BindVertexArray(Vao);
    }

    public static void Terminate()
    {
        GL.DeleteVertexArray(Vao);
    }

    ~ChunkTessellator()
    {
        GL.UnmapNamedBuffer(_drawElementsIndirectCommandsBuffer);
        GL.DeleteBuffer(_drawElementsIndirectCommandsBuffer);
        GL.UnmapNamedBuffer(_chunksTransformsBuffer);
        GL.DeleteBuffer(_chunksTransformsBuffer);
    }

    public ChunkTessellator(ushort chunksCount)
    {
        _chunksCount = chunksCount;
        
        GL.CreateBuffers(1, out _chunksTransformsBuffer);
        var helper = chunksCount * Marshal.SizeOf<Matrix4>();
        GL.NamedBufferStorage(_chunksTransformsBuffer,
            helper, new byte[helper],
            BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        _chunksTransformsPointer = GL.MapNamedBufferRange(_chunksTransformsBuffer, 0, helper,
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, _chunksTransformsBuffer);
        
        GL.CreateBuffers(1, out _drawElementsIndirectCommandsBuffer);
        helper = 3 * chunksCount * Marshal.SizeOf<GlDrawElementsIndirectCommand>();
        GL.NamedBufferStorage(_drawElementsIndirectCommandsBuffer,
            helper, new byte[helper],
            BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        _drawElementsIndirectCommandsPointer = GL.MapNamedBufferRange(_drawElementsIndirectCommandsBuffer, 0, helper,
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
        for (ushort i = 0; i < chunksCount; i++)
        {
            GlDrawElementsIndirectCommand command = new()
            {
                baseVertex = (uint)i * VboBufferSize / sizeof(uint),
                firstIndex = (uint)i * EboBufferSize / sizeof(uint),
                instanceCount = 1
            };
            Marshal.Copy(EngineDefaults.GetBytes(command).ToArray(), 0,
                _drawElementsIndirectCommandsPointer + 3 * i * Marshal.SizeOf<GlDrawElementsIndirectCommand>(),
                Marshal.SizeOf<GlDrawElementsIndirectCommand>());
            Marshal.Copy(EngineDefaults.GetBytes(command).ToArray(), 0,
                _drawElementsIndirectCommandsPointer + (3 * i + 1) * Marshal.SizeOf<GlDrawElementsIndirectCommand>(),
                Marshal.SizeOf<GlDrawElementsIndirectCommand>());
            Marshal.Copy(EngineDefaults.GetBytes(command).ToArray(), 0,
                _drawElementsIndirectCommandsPointer + (3 * i + 2) * Marshal.SizeOf<GlDrawElementsIndirectCommand>(),
                Marshal.SizeOf<GlDrawElementsIndirectCommand>());
        }
        
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

    public void SetTriangles(ushort chunkId, uint[] triangles0, uint[] triangles1, uint[] triangles2)
    {
        var i = chunkId * EboBufferSize / sizeof(uint);
        Marshal.Copy(BitConverter.GetBytes(triangles0.Length), 0,
            _drawElementsIndirectCommandsPointer + 3 * chunkId * Marshal.SizeOf<GlDrawElementsIndirectCommand>(),
            sizeof(uint));
        
        Marshal.Copy(BitConverter.GetBytes(triangles1.Length), 0,
            _drawElementsIndirectCommandsPointer + (3 * chunkId + 1) * Marshal.SizeOf<GlDrawElementsIndirectCommand>(),
            sizeof(uint));
        Marshal.Copy(BitConverter.GetBytes(chunkId * EboBufferSize / sizeof(uint) + triangles0.Length), 0,
            _drawElementsIndirectCommandsPointer + (3 * chunkId + 1) * Marshal.SizeOf<GlDrawElementsIndirectCommand>() +
            8, sizeof(uint));
        
        Marshal.Copy(BitConverter.GetBytes(triangles2.Length), 0,
            _drawElementsIndirectCommandsPointer + (3 * chunkId + 2) * Marshal.SizeOf<GlDrawElementsIndirectCommand>(),
            sizeof(uint));
        Marshal.Copy(
            BitConverter.GetBytes(chunkId * EboBufferSize / sizeof(uint) + triangles0.Length + triangles1.Length), 0,
            _drawElementsIndirectCommandsPointer + (3 * chunkId + 2) * Marshal.SizeOf<GlDrawElementsIndirectCommand>() +
            8, sizeof(uint));
        
        for (var j = 0; j < triangles0.Length; i++, j++) Marshal.Copy(BitConverter.GetBytes(triangles0[j]), 0, _eboPointer + i * sizeof(uint), sizeof(uint));
        for (var j = 0; j < triangles1.Length; i++, j++) Marshal.Copy(BitConverter.GetBytes(triangles1[j]), 0, _eboPointer + i * sizeof(uint), sizeof(uint));
        for (var j = 0; j < triangles2.Length; i++, j++) Marshal.Copy(BitConverter.GetBytes(triangles2[j]), 0, _eboPointer + i * sizeof(uint), sizeof(uint));
    }

    public override void Draw()
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, _drawElementsIndirectCommandsBuffer);
        GL.BindVertexArray(Vao);
        GL.MultiDrawElementsIndirect(PrimitiveType.Points, DrawElementsType.UnsignedInt, IntPtr.Zero, _chunksCount * 3, Marshal.SizeOf<GlDrawElementsIndirectCommand>());
    }

    public void SetVertex(ushort chunkId, ushort index, BlockType blockType, byte light)
    {
        var finalIndex = chunkId * VboBufferSize / sizeof(uint) + index;
        var constructedVertex = (uint)blockType << 16;
        constructedVertex |= (uint)light << 10;
        Marshal.Copy(BitConverter.GetBytes(constructedVertex), 0, _vboPointer + finalIndex * sizeof(uint), sizeof(uint));
    }
    
    public void SetChunkTransform(ushort chunkId, Matrix4 transform)
    {
        Marshal.Copy(EngineDefaults.GetBytes(transform).ToArray(), 0, _chunksTransformsPointer + chunkId * Marshal.SizeOf<Matrix4>(), Marshal.SizeOf<Matrix4>());
    }
}