using MinecraftClient.Render.Entities.Player;
using MinecraftClient.Render.Shaders;
using MinecraftClient.Render.World.Block;
using MinecraftLibrary.Engine.States.World;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.World;

public class WorldRenderer
{
    private int FogsBuffer;
    private HashSet<ChunkRenderer> _chunkRenderers = new();
    private HashSet<ChunkRenderer> _dirtyChunkRenderers = new();

    public static readonly BlockRenderer[] BlockRenderers = new[] { new AirBlockRenderer() };
    private void InitFog()
    {
        GL.CreateBuffers(1, out FogsBuffer);
        float[] fogs =
        {
            14.0f / 255.0f, 11.0f / 255.0f, 10.0f / 255.0f, 1.0f, 0.01f, 0.0f, 0.0f, 0.0f, 254.0f / 255.0f,
            251.0f / 255.0f, 250.0F / 255.0f, 1.0f, 0.001f, 0.0f, 0.0f, 0.0f
        };

        GL.NamedBufferStorage(FogsBuffer, fogs.Length * sizeof(float), fogs, BufferStorageFlags.None);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, FogsBuffer);
    }
    public WorldRenderer()
    {
        MinecraftLibrary.Engine.World.GetInstance()!.OnChunkAdded += OnChunkAdded;
        Shader.MainShader.SetUnsignedInt("worldTime", (uint)MinecraftLibrary.Engine.World.GetInstance()!.State.WorldTime);
        InitFog();
    }

    ~WorldRenderer()
    {
        GL.DeleteBuffer(FogsBuffer);
    }

    private void OnChunkAdded(ChunkState state)
    {
        var renderer = new ChunkRenderer(state);
        _chunkRenderers.Add(renderer);
        renderer.OnChunkRenderDirty += OnChunkRenderDirty;
    }

    private void OnChunkRenderDirty(ChunkRenderer renderer)
    {
        _dirtyChunkRenderers.Add(renderer);
    }

    public void HandleWindowResize(int height, int width)
    {
        GL.Viewport(0, 0, width, height);
        Camera.GetInstance().SetAspectRatio((float)width / height);
    }

    public void Render()
    {
        var camera = Camera.GetInstance();
        var cameraFrustum = camera.GetFrustum();
        Shader.MainShader.SetMat4("view", camera.GetViewMatrix());
        Shader.MainShader.SetMat4("projection", camera.GetProjectionMatrix());
    }
}