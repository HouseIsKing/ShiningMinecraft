using OpenTK.Graphics.OpenGL4;

namespace MinecraftClient.Render;

public abstract class Tessellator
{
    protected uint Vao { get; }
    protected uint Ebo { get; }
    protected uint Vbo { get; }
    protected uint DrawElementsIndirectCommandsBuffer { get; }

    protected Tessellator()
    {
        GL.CreateVertexArrays(1, out uint helper);
        Vao = helper;
        var helper2 = new uint[3];
        GL.CreateBuffers(3, helper2);
        Ebo = helper2[0];
        Vbo = helper2[1];
        DrawElementsIndirectCommandsBuffer = helper2[2];
        GL.VertexArrayElementBuffer(Vao, Ebo);
    }
    
    ~Tessellator()
    {
        GL.DeleteVertexArray(Vao);
        GL.DeleteBuffer(Ebo);
        GL.DeleteBuffer(Vbo);
        GL.DeleteBuffer(DrawElementsIndirectCommandsBuffer);
    }

    internal void PrepareToDraw()
    {
        GL.BindVertexArray(Vao);
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, DrawElementsIndirectCommandsBuffer);
    }

    internal abstract void Draw();
}