using System.Runtime.InteropServices;
using MinecraftClient.Render.Shaders;
using MinecraftLibrary.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Buffer = System.Buffer;

namespace MinecraftClient.Render.World;

public class ChunkTessellator : Tessellator
{
    private IntPtr VboPointer;
    private IntPtr EboPointer;

    private static readonly uint Vao;
    private static readonly uint VboPos;
    private readonly uint _vbo;
    private readonly uint[] _vbos;

    static ChunkTessellator()
    {
        GL.CreateVertexArrays(1, out Vao);
        GL.CreateBuffers(1, out VboPos);
        GL.NamedBufferStorage(VboPos, EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * 3 * 3 * sizeof(float), GetVertexesPos(), BufferStorageFlags.None);
        GL.VertexArrayAttribFormat(Vao, 0, 3, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribFormat(Vao, 1, 2, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribFormat(Vao, 2, 4, VertexAttribType.Float, false, 8);
        GL.VertexArrayAttribIFormat(Vao, 3, 1, VertexAttribIntegerType.UnsignedShort, 24);
        GL.VertexArrayAttribIFormat(Vao, 4, 1, VertexAttribIntegerType.UnsignedByte, 26);
        GL.VertexArrayAttribIFormat(Vao, 5, 1, VertexAttribIntegerType.UnsignedByte, 27);
        GL.VertexArrayAttribBinding(Vao, 0, 0);
        GL.VertexArrayAttribBinding(Vao, 1, 1);
        GL.VertexArrayAttribBinding(Vao, 2, 1);
        GL.VertexArrayAttribBinding(Vao, 3, 1);
        GL.VertexArrayAttribBinding(Vao, 4, 1);
        GL.VertexArrayAttribBinding(Vao, 5, 1);
        GL.VertexArrayVertexBuffer(Vao, 0, VboPos, 0, 3 * sizeof(float));
        GL.EnableVertexArrayAttrib(Vao, 0);
        GL.EnableVertexArrayAttrib(Vao, 1);
        GL.EnableVertexArrayAttrib(Vao, 2);
        GL.EnableVertexArrayAttrib(Vao, 3);
        GL.EnableVertexArrayAttrib(Vao, 4);
        GL.EnableVertexArrayAttrib(Vao, 5);
    }

    public static void Terminate()
    {
        GL.DeleteBuffer(VboPos);
        GL.DeleteVertexArray(Vao);
    }

    private static Vector3[] GetVertexesPos()
    {
        var result = new Vector3[EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * 3];
        for (var i = 0; i < EngineDefaults.ChunkWidth; i++)
        for (var j = 0; j < EngineDefaults.ChunkHeight; j++)
        for (var k = 0; k < EngineDefaults.ChunkDepth; k++)
        {
            Vector3 pos = new(i, j, k);
            result[i * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth + j * EngineDefaults.ChunkDepth + k] = pos;
            result[
                i * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth + j * EngineDefaults.ChunkDepth + k +
                1] = pos;
            result[i * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth + j * EngineDefaults.ChunkDepth +
                   k + 2] = pos;
        }

        return result;
    }

    public ChunkTessellator()
    {
        GL.CreateBuffers(1, out _vbo);
        _vbos = new[] { _vbo, _vbo, _vbo, _vbo, _vbo };
        GL.NamedBufferStorage(_vbo, EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * 3 * Marshal.SizeOf<ChunkVertex>(), IntPtr.Zero, BufferStorageFlags.MapWriteBit);
        VboPointer = GL.MapNamedBuffer(_vbo, BufferAccess.WriteOnly);
        GL.NamedBufferStorage(Ebo, EngineDefaults.ChunkWidth / 2 * EngineDefaults.ChunkHeight / 2 * EngineDefaults.ChunkDepth / 2 * 6 * 6 * sizeof(ushort), IntPtr.Zero, BufferStorageFlags.MapWriteBit);
        EboPointer = GL.MapNamedBuffer(Ebo, BufferAccess.WriteOnly);
    }

    ~ChunkTessellator()
    {
        GL.UnmapNamedBuffer(_vbo);
        GL.UnmapNamedBuffer(Ebo);
        GL.DeleteBuffer(_vbo);
    }

    public override void Draw()
    {
        Shader.MainShader.SetMat4("transformationMatrix", ModelMatrix);
        GL.VertexArrayVertexBuffer(Vao, 1u, _vbo, 0, Marshal.SizeOf<ChunkVertex>());
        GL.VertexArrayElementBuffer(Vao, Ebo);
        GL.DrawElements(PrimitiveType.Triangles, TrianglesCount, DrawElementsType.UnsignedShort, 0);
    }

    public void SetVertex(ushort index, byte offset, ChunkVertex vertex)
    {
        Marshal.Copy(ChunkVertex.ToBytes(vertex), 0, VboPointer + (index * 3 + offset) * Marshal.SizeOf<ChunkVertex>(),
            Marshal.SizeOf<ChunkVertex>());
    }

    public void SetTriangles(ushort[] triangles)
    {
        TrianglesCount = triangles.Length;
        var bytes = new byte[triangles.Length * sizeof(ushort)];
        Buffer.BlockCopy(triangles, 0, bytes, 0, bytes.Length);
        Marshal.Copy(bytes, 0, EboPointer, triangles.Length);
    }
}