using System.Runtime.InteropServices;
using MinecraftClient.Render.Shaders;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Blocks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Buffer = System.Buffer;

namespace MinecraftClient.Render.World;

public sealed class ChunkTessellator : Tessellator
{
    private IntPtr _vboPointer;
    private IntPtr _eboPointer;
    private readonly int[] _trianglesCount = { 0, 0, 0, 0, 0, 0 };
    private readonly IntPtr[] _trianglesOffset = { 0, 0, 0, 0, 0, 0 };

    private static readonly uint Vao;

    static ChunkTessellator()
    {
        GL.CreateVertexArrays(1, out Vao);
        GL.EnableVertexArrayAttrib(Vao, 0);
        GL.VertexArrayAttribFormat(Vao, 0, 1, VertexAttribType.UnsignedInt, false, 0);
        GL.VertexArrayAttribBinding(Vao, 0, 0);
        GL.BindVertexArray(Vao);
    }

    public static void Terminate()
    {
        GL.DeleteVertexArray(Vao);
    }

    public ChunkTessellator()
    {
        GL.NamedBufferStorage(Vbo, EngineDefaults.ChunkWidth * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * 4 * 4, IntPtr.Zero, BufferStorageFlags.MapWriteBit);
        GL.NamedBufferStorage(Ebo, EngineDefaults.ChunkWidth / 2 * EngineDefaults.ChunkHeight * EngineDefaults.ChunkDepth * 36 * sizeof(uint), IntPtr.Zero, BufferStorageFlags.MapWriteBit);
    }

    public override void Draw()
    {
        if (TrianglesCount == 0) return;
        Shader.MainShader.SetMat4("transformationMatrix", ModelMatrix);
        GL.VertexArrayVertexBuffer(Vao, 0u, Vbo, 0, sizeof(uint));
        GL.VertexArrayElementBuffer(Vao, Ebo);
        GL.MultiDrawElements(PrimitiveType.Triangles, _trianglesCount, DrawElementsType.UnsignedInt, _trianglesOffset, 6);
    }

    public void SetTriangles(uint[] triangles0, uint[] triangles1, uint[] triangles2, uint[] triangles3, uint[] triangles4, uint[] triangles5)
    {
        _eboPointer = GL.MapNamedBuffer(Ebo, BufferAccess.WriteOnly);
        _trianglesCount[0] = triangles0.Length * 6;
        _trianglesCount[1] = triangles1.Length * 6;
        _trianglesCount[2] = triangles2.Length * 6;
        _trianglesCount[3] = triangles3.Length * 6;
        _trianglesCount[4] = triangles4.Length * 6;
        _trianglesCount[5] = triangles5.Length * 6;
        _trianglesOffset[0] = IntPtr.Zero;
        _trianglesOffset[1] = _trianglesCount[0] * sizeof(uint);
        _trianglesOffset[2] = (_trianglesCount[0] + _trianglesCount[1]) * sizeof(uint);
        _trianglesOffset[3] = (_trianglesCount[0] + _trianglesCount[1] + _trianglesCount[2]) * sizeof(uint);
        _trianglesOffset[4] = (_trianglesCount[0] + _trianglesCount[1] + _trianglesCount[2] + _trianglesCount[3]) * sizeof(uint);
        _trianglesOffset[5] = (_trianglesCount[0] + _trianglesCount[1] + _trianglesCount[2] + _trianglesCount[3] + _trianglesCount[4]) * sizeof(uint);
        for (var i = 0; i < triangles0.Length; i++)
        {
            var bytes0 = BitConverter.GetBytes(triangles0[i] * 4);
            var bytes1 = BitConverter.GetBytes(triangles0[i] * 4 + 1);
            var bytes2 = BitConverter.GetBytes(triangles0[i] * 4 + 2);
            var bytes3 = BitConverter.GetBytes(triangles0[i] * 4 + 3);
            var offsetHelper = i * 6;
            Marshal.Copy(bytes0, 0, _eboPointer + offsetHelper * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 1) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 2) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes3, 0, _eboPointer + (offsetHelper + 3) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 4) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 5) * sizeof(uint), sizeof(uint));
        }
        for (var i = 0; i < triangles1.Length; i++)
        {
            var bytes0 = BitConverter.GetBytes(triangles1[i] * 4);
            var bytes1 = BitConverter.GetBytes(triangles1[i] * 4 + 1);
            var bytes2 = BitConverter.GetBytes(triangles1[i] * 4 + 2);
            var bytes3 = BitConverter.GetBytes(triangles1[i] * 4 + 3);
            var offsetHelper = i * 6 + _trianglesCount[0];
            Marshal.Copy(bytes0, 0, _eboPointer + offsetHelper * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 1) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 2) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes3, 0, _eboPointer + (offsetHelper + 3) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 4) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 5) * sizeof(uint), sizeof(uint));
        }
        for (var i = 0; i < triangles2.Length; i++)
        {
            var bytes0 = BitConverter.GetBytes(triangles2[i] * 4);
            var bytes1 = BitConverter.GetBytes(triangles2[i] * 4 + 1);
            var bytes2 = BitConverter.GetBytes(triangles2[i] * 4 + 2);
            var bytes3 = BitConverter.GetBytes(triangles2[i] * 4 + 3);
            var offsetHelper = i * 6 + _trianglesCount[0] + _trianglesCount[1];
            Marshal.Copy(bytes0, 0, _eboPointer + offsetHelper * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 1) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 2) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes3, 0, _eboPointer + (offsetHelper + 3) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 4) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 5) * sizeof(uint), sizeof(uint));
        }
        for (var i = 0; i < triangles3.Length; i++)
        {
            var bytes0 = BitConverter.GetBytes(triangles3[i] * 4);
            var bytes1 = BitConverter.GetBytes(triangles3[i] * 4 + 1);
            var bytes2 = BitConverter.GetBytes(triangles3[i] * 4 + 2);
            var bytes3 = BitConverter.GetBytes(triangles3[i] * 4 + 3);
            var offsetHelper = i * 6 + _trianglesCount[0] + _trianglesCount[1] + _trianglesCount[2];
            Marshal.Copy(bytes0, 0, _eboPointer + offsetHelper * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 1) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 2) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes3, 0, _eboPointer + (offsetHelper + 3) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 4) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 5) * sizeof(uint), sizeof(uint));
        }
        for (var i = 0; i < triangles4.Length; i++)
        {
            var bytes0 = BitConverter.GetBytes(triangles4[i] * 4);
            var bytes1 = BitConverter.GetBytes(triangles4[i] * 4 + 1);
            var bytes2 = BitConverter.GetBytes(triangles4[i] * 4 + 2);
            var bytes3 = BitConverter.GetBytes(triangles4[i] * 4 + 3);
            var offsetHelper = i * 6 + _trianglesCount[0] + _trianglesCount[1] + _trianglesCount[2] + _trianglesCount[3];
            Marshal.Copy(bytes0, 0, _eboPointer + offsetHelper * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 1) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 2) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes3, 0, _eboPointer + (offsetHelper + 3) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 4) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 5) * sizeof(uint), sizeof(uint));
        }
        for (var i = 0; i < triangles5.Length; i++)
        {
            var bytes0 = BitConverter.GetBytes(triangles5[i] * 4);
            var bytes1 = BitConverter.GetBytes(triangles5[i] * 4 + 1);
            var bytes2 = BitConverter.GetBytes(triangles5[i] * 4 + 2);
            var bytes3 = BitConverter.GetBytes(triangles5[i] * 4 + 3);
            var offsetHelper = i * 6 + _trianglesCount[0] + _trianglesCount[1] + _trianglesCount[2] + _trianglesCount[3] + _trianglesCount[4];
            Marshal.Copy(bytes0, 0, _eboPointer + offsetHelper * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 1) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 2) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes3, 0, _eboPointer + (offsetHelper + 3) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes2, 0, _eboPointer + (offsetHelper + 4) * sizeof(uint), sizeof(uint));
            Marshal.Copy(bytes1, 0, _eboPointer + (offsetHelper + 5) * sizeof(uint), sizeof(uint));
        }
        TrianglesCount = _trianglesCount[0] + _trianglesCount[1] + _trianglesCount[2] + _trianglesCount[3] + _trianglesCount[4] + _trianglesCount[5];
        GL.UnmapNamedBuffer(Ebo);
    }

    public void BeginUpdateVertex()
    {
        _vboPointer = GL.MapNamedBuffer(Vbo, BufferAccess.WriteOnly);
    }

    public void EndUpdateVertex()
    {
        GL.UnmapNamedBuffer(Vbo);
    }

    public void SetVertex(ushort index, BlockType blockType, byte light)
    {
        var constructedVertex = (uint)blockType << 16;
        constructedVertex |= (uint)light << 10;
        var bits = BitConverter.GetBytes(constructedVertex);
        Marshal.Copy(bits, 0, _vboPointer + index * 4 * sizeof(uint), sizeof(uint));
        Marshal.Copy(bits, 0, _vboPointer + (index * 4 + 1) * sizeof(uint), sizeof(uint));
        Marshal.Copy(bits, 0, _vboPointer + (index * 4 + 2) * sizeof(uint), sizeof(uint));
        Marshal.Copy(bits, 0, _vboPointer + (index * 4 + 3) * sizeof(uint), sizeof(uint));
    }
}