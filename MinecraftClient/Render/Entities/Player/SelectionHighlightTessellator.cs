using System.Runtime.InteropServices;
using MinecraftClient.Render.Shaders;
using MinecraftLibrary.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.Entities.Player;

internal sealed class SelectionHighlightTessellator : Tessellator
{
    private static readonly byte[] VertexHelper = new byte[4];
    private readonly IntPtr _vboPointer;
    private Vector3 _position;
    public bool Mode { private get; set; }

    public SelectionHighlightTessellator()
    {
        GL.EnableVertexArrayAttrib(Vao, 0);
        GL.VertexArrayAttribIFormat(Vao, 0, 1, VertexAttribIntegerType.UnsignedInt, 0);
        GL.VertexArrayAttribBinding(Vao, 0, 0);
        GlDrawElementsIndirectCommand command = new()
        {
            BaseInstance = 0,
            Count = 1,
            InstanceCount = 1,
            FirstIndex = 0,
            BaseVertex = 0
        };
        GL.NamedBufferStorage(DrawElementsIndirectCommandsBuffer, Marshal.SizeOf<GlDrawElementsIndirectCommand>(),
            ref command, BufferStorageFlags.None);
        GL.NamedBufferStorage(Ebo, sizeof(byte), new byte[] { 0 }, BufferStorageFlags.None);
        GL.NamedBufferStorage(Vbo, sizeof(uint), VertexHelper, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        _vboPointer = GL.MapNamedBufferRange(Vbo, 0, sizeof(uint), BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
        GL.VertexArrayVertexBuffer(Vao, 0, Vbo, IntPtr.Zero, sizeof(uint));
    }
    
    ~SelectionHighlightTessellator()
    {
        GL.UnmapNamedBuffer(Vbo);
    }

    internal override void Draw()
    {
        float alpha;
        if (Mode)
        {
            Shader.SelectionHighlightBShader.Use();
            var bright = (float)(Math.Sin(EngineDefaults.MilliTime() / 100.0) * 0.2) + 0.8F;
            alpha = (float)(Math.Sin(EngineDefaults.MilliTime() / 200.0) * 0.2) + 0.5F;
            Shader.SelectionHighlightBShader.SetFloat("alpha", alpha);
            Shader.SelectionHighlightBShader.SetVector3("hitPos", _position);
            Shader.SelectionHighlightBShader.SetFloat("bright", bright);
        }
        else
        {
            Shader.SelectionHighlightAShader.Use();
            alpha = ((float)(Math.Sin(EngineDefaults.MilliTime() / 100.0) * 0.2) + 0.4f) * 0.5f;
            Shader.SelectionHighlightAShader.SetFloat("alpha", alpha);
            Shader.SelectionHighlightAShader.SetVector3("hitPos", _position);
        }

        GL.DrawElementsIndirect(PrimitiveType.Points, DrawElementsType.UnsignedByte, IntPtr.Zero);
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