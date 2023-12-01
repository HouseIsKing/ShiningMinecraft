using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render;

public abstract class Tessellator
{
    protected uint Ebo { get; }
    
    protected uint Vbo { get; }

    protected int TrianglesCount = 0;

    protected Tessellator()
    {
        GL.CreateBuffers(1, out uint helper);
        Ebo = helper;
        GL.CreateBuffers(1, out helper);
        Vbo = helper;
    }
    
    ~Tessellator()
    {
        GL.DeleteBuffer(Ebo);
        GL.DeleteBuffer(Vbo);
    }

    public abstract void Draw();
}