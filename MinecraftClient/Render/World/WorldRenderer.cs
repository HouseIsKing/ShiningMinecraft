using System.Runtime.InteropServices;
using MinecraftClient.Render.Entities.Player;
using MinecraftClient.Render.Shaders;
using MinecraftClient.Render.Textures;
using MinecraftClient.Render.World.Block;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.States.World;
using MinecraftLibrary.Network;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MinecraftClient.Render.World;

public sealed class WorldRenderer
{
    private uint FogsBuffer;
    private uint WorldInfoBuffer;
    private IntPtr _worldInfoPointer;
    private uint CameraInfoBuffer;
    private IntPtr _cameraInfoPointer;
    private readonly ChunkRenderer[,,] _chunkRenderers = new ChunkRenderer[MinecraftLibrary.Engine.World.WorldWidth / EngineDefaults.ChunkWidth, MinecraftLibrary.Engine.World.WorldHeight / EngineDefaults.ChunkHeight, MinecraftLibrary.Engine.World.WorldDepth / EngineDefaults.ChunkDepth];
    private readonly ChunkTessellator _chunkTessellator;
    private PlayerRenderer _playerRenderer;
    
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
    
    private void InitWorldInfo()
    {
        GL.CreateBuffers(1, out WorldInfoBuffer);
        uint[] worldInfo =
        {
            EngineDefaults.ChunkWidth, EngineDefaults.ChunkHeight, EngineDefaults.ChunkDepth, (uint)MinecraftLibrary.Engine.World.GetInstance()!.GetWorldTime()
        };

        GL.NamedBufferStorage(WorldInfoBuffer, worldInfo.Length * sizeof(uint), worldInfo, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 6, WorldInfoBuffer);
        _worldInfoPointer = GL.MapNamedBufferRange(WorldInfoBuffer, 3 * sizeof(uint), sizeof(uint),
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
    }

    private void InitCameraInfo()
    {
        GL.CreateBuffers(1, out CameraInfoBuffer);
        Matrix4[] cameraInfo =
        {
            Camera.GetInstance().GetViewMatrix(), Camera.GetInstance().GetProjectionMatrix()
        };
        GL.NamedBufferStorage(CameraInfoBuffer, cameraInfo.Length * Marshal.SizeOf<Matrix4>(), cameraInfo,
            BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 5, CameraInfoBuffer);
        _cameraInfoPointer = GL.MapNamedBufferRange(CameraInfoBuffer, 0, 2 * Marshal.SizeOf<Matrix4>(),
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
    }
    public WorldRenderer(PlayerRenderer playerRenderer)
    {
        _playerRenderer = playerRenderer;
        var world = MinecraftLibrary.Engine.World.GetInstance()!;
        _chunkTessellator = new ChunkTessellator(MinecraftLibrary.Engine.World.GetMaxChunksCount());
        world.OnChunkAdded += OnChunkAdded;
        Shader.MainShader.Use();
        InitFog();
        InitCameraInfo();
        InitWorldInfo();
        BlockRenderer.Setup();
        Texture.SetupTextures();
    }

    ~WorldRenderer()
    {
        BlockRenderer.Terminate();
        Texture.Terminate();
        GL.DeleteBuffer(FogsBuffer);
        GL.UnmapNamedBuffer(WorldInfoBuffer);
        GL.DeleteBuffer(WorldInfoBuffer);
        GL.UnmapNamedBuffer(CameraInfoBuffer);
        GL.DeleteBuffer(CameraInfoBuffer);
    }

    private void OnChunkAdded(ChunkState state, Vector3i index)
    {
        var renderer = new ChunkRenderer(state, (ushort)(index.X * _chunkRenderers.GetLength(2) * _chunkRenderers.GetLength(1) + index.Y * _chunkRenderers.GetLength(2) + index.Z));
        _chunkTessellator.SetChunkTransform(renderer.ChunkId, renderer.ModelMatrix);
        _chunkRenderers[index.X, index.Y, index.Z] = renderer;
    }

    public void HandleWindowResize(int height, int width)
    {
        GL.Viewport(0, 0, width, height);
        Camera.GetInstance().SetAspectRatio((float)width / height);
    }

    public void Render(float delta)
    {
        _playerRenderer.UpdateRenderer(delta);
        var camera = Camera.GetInstance();
        var cameraFrustum = camera.GetFrustum();
        Marshal.Copy(EngineDefaults.GetBytes(camera.GetViewMatrix()).ToArray(), 0, _cameraInfoPointer, Marshal.SizeOf<Matrix4>());
        Marshal.Copy(EngineDefaults.GetBytes(camera.GetProjectionMatrix()).ToArray(), 0, _cameraInfoPointer + Marshal.SizeOf<Matrix4>(), Marshal.SizeOf<Matrix4>());
        var world = MinecraftLibrary.Engine.World.GetInstance()!;
        Marshal.Copy(EngineDefaults.GetBytes((uint)world.GetWorldTime()).ToArray(), 0, _worldInfoPointer, sizeof(uint));
        var dirtyChunksEnumerator = _chunkRenderers.GetEnumerator();
        for (var i = 0; i < 8 && dirtyChunksEnumerator.MoveNext();)
        {
            var chunkRenderer = (ChunkRenderer)dirtyChunksEnumerator.Current;
            if (chunkRenderer == null || !chunkRenderer.IsDirty()) continue;
            chunkRenderer.UpdateRenderer(_chunkTessellator);
            i++;
        }

        _chunkTessellator.Draw();
    }

    private void ApplyTickChunksChanges(Packet changePacket)
    {
        changePacket.Read(out ushort chunksCount);
        for (ushort i = 0; i < chunksCount; i++)
        {
            changePacket.Read(out byte a);
            changePacket.Read(out byte b);
            changePacket.Read(out byte c);
            _chunkRenderers[a, b, c].UpdateRendererChanges(changePacket);
        }
    }
    
    public void ApplyTickChanges(Packet changePacket)
    {
        changePacket.Read(out ulong worldTime);
        Marshal.Copy(BitConverter.GetBytes((uint)worldTime), 0, _worldInfoPointer, sizeof(uint));
        changePacket.Read(out long _);
        ApplyTickChunksChanges(changePacket);
        ApplyTickLightChanges(changePacket);
        ApplyTickPlayerChanges(changePacket);
        changePacket.ResetRead();
    }

    private void ApplyTickPlayerChanges(Packet changePacket)
    {
        changePacket.Read(out ushort playerCount);
        for (var i = 0; i < playerCount; i++)
        {
            changePacket.Read(out ushort playerId);
            if (playerId == _playerRenderer.GetPlayerId())
            {
                _playerRenderer.ApplyRevertChanges(changePacket);
            }
            else
            {
            }
        }
    }

    private void ApplyTickLightChanges(Packet changePacket)
    {
        changePacket.Read(out ushort lightCount);
        for (var i = 0; i < lightCount; i++)
        {
            changePacket.Read(out ushort _);
            changePacket.Read(out ushort _);
            changePacket.Read(out byte _);
        }
    }
}