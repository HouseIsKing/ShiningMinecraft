using System.Runtime.InteropServices;
using MinecraftClient.Render.Shaders;
using MinecraftLibrary.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.Entities.Player;

public class SelectionHighlightTessellator : Tessellator
{
    private static readonly uint Vao;
    private static readonly byte[] VertexHelper = new byte[4];
    private IntPtr _vboPointer;
    private static readonly uint commandBuffer;
    private Vector3 _position;
    
    static SelectionHighlightTessellator()
    {
        GlDrawArraysIndirectCommand command = new()
        {
            Count = 1,
            InstanceCount = 1,
            First = 0,
            BaseInstance = 0
        };
        GL.CreateVertexArrays(1, out Vao);
        GL.EnableVertexArrayAttrib(Vao, 0);
        GL.VertexArrayAttribIFormat(Vao, 0, 1, VertexAttribIntegerType.UnsignedInt, 0);
        GL.VertexArrayAttribBinding(Vao, 0, 0);
        GL.BindVertexArray(Vao);
        GL.CreateBuffers(1, out commandBuffer);
        GL.NamedBufferStorage(commandBuffer, Marshal.SizeOf<GlDrawArraysIndirectCommand>(), ref command, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, commandBuffer);
    }

    public SelectionHighlightTessellator()
    {
        GL.NamedBufferStorage(Vbo, sizeof(uint), VertexHelper, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        _vboPointer = GL.MapNamedBufferRange(Vbo, 0, sizeof(uint), BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
        GL.VertexArrayVertexBuffer(Vao, 0, Vbo, IntPtr.Zero, sizeof(uint));
    }
    public override void Draw()
    {
        Shader.SelectionHighlightAShader.Use();
        var alpha = ((float)Math.Sin(DateTime.Now.Millisecond / 100.0) * 0.2f + 0.4f) * 0.5f;
        Shader.SelectionHighlightAShader.SetFloat("alpha", alpha);
        Shader.SelectionHighlightAShader.SetVector3("hitPos", _position);
        GL.BindVertexArray(Vao);
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, commandBuffer);
        GL.DrawArraysIndirect(PrimitiveType.Points, IntPtr.Zero);
    }
    
    public void SetVertex(BlockType blockType, byte light, Vector3 position)
    {
        var constructedVertex = (uint)blockType << 16;
        constructedVertex |= (uint)light << 10;
        VertexHelper[3] = (byte)(constructedVertex >> 24);
        VertexHelper[2] = (byte)((constructedVertex >> 16) & 0xFF);
        VertexHelper[1] = (byte)((constructedVertex >> 8) & 0xFF);
        VertexHelper[0] = (byte)(constructedVertex & 0xFF);
        Marshal.Copy(VertexHelper, 0, _vboPointer, sizeof(uint));
        _position = position;
    }
}