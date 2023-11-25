using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render;

public abstract class Tessellator
{
    protected uint Ebo { get; }
    
    protected uint Vbo { get; }

    protected int TrianglesCount = 0;
    
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;
    
    public Tessellator Parent { get; }

    public Matrix4 ModelMatrix => (Parent == this ? Matrix4.Identity : Parent.ModelMatrix) * Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);

    protected Tessellator()
    {
        Parent = this;
        GL.CreateBuffers(1, out uint helper);
        Ebo = helper;
        GL.CreateBuffers(1, out helper);
        Vbo = helper;
    }

    protected Tessellator(Tessellator parent)
    {
        Parent = parent;
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