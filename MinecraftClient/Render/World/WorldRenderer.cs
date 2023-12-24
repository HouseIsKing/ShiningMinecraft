using System.Diagnostics;
using System.Runtime.InteropServices;
using MinecraftClient.Render.Entities.Player;
using MinecraftClient.Render.Shaders;
using MinecraftClient.Render.Textures;
using MinecraftClient.Render.World.Block;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Network;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Buffer = System.Buffer;

namespace MinecraftClient.Render.World;

public sealed class WorldRenderer
{
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly uint _msFbo;
    private uint _msColorRenderBuffer;
    private uint _msDepthRenderBuffer;
    private readonly uint _fbo;
    private uint _frameBufferColorTexture;
    private uint _frameBufferDepthTexture;
    private Vector2i _screenSize;
    private uint _fogsBuffer;
    private uint _worldInfoBuffer;
    private IntPtr _worldInfoPointer;
    private uint _cameraInfoBuffer;
    private IntPtr _cameraInfoPointer;
    private readonly ChunkRenderer[,,] _chunkRenderers = new ChunkRenderer[MinecraftLibrary.Engine.World.WorldWidth / EngineDefaults.ChunkWidth, MinecraftLibrary.Engine.World.WorldHeight / EngineDefaults.ChunkHeight, MinecraftLibrary.Engine.World.WorldDepth / EngineDefaults.ChunkDepth];
    private readonly ChunkTessellator _chunkTessellator;
    private readonly List<ChunkRenderer> _visibleChunkRenderers = [];
    private readonly byte[] _matrixBuffer = new byte[Marshal.SizeOf<Matrix4>() * 2];
    internal PlayerRenderer PlayerRenderer { get; }
    private Dictionary<ushort, OtherPlayerRenderer> OtherPlayers { get; } = new();
    private const int RenderWidth = MinecraftLibrary.Engine.World.WorldWidth;
    private const int RenderHeight = MinecraftLibrary.Engine.World.WorldHeight;
    private const int RenderDepth = MinecraftLibrary.Engine.World.WorldDepth;
    internal Vector3i BaseVector { get; } = MinecraftLibrary.Engine.World.Instance.GetBaseVector();
    internal static WorldRenderer Instance { get; private set; }

    private void InitWorldFrameBuffer(int width, int height)
    {
        _screenSize = new Vector2i(width, height);
        float[] quadVertices =
        [
            // positions        // texture Coords
            -1.0f, 1.0f, 1.0f, 0.0f, 1.0f,
            -1.0f, -1.0f, 1.0f, 0.0f, 0.0f,
            1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
            1.0f, -1.0f, 1.0f, 1.0f, 0.0f
        ];
        GL.NamedBufferStorage(_vbo, 4 * 5 * sizeof(float), quadVertices, BufferStorageFlags.None);
        GL.VertexArrayVertexBuffer(_vao, 0, _vbo, IntPtr.Zero, 5 * sizeof(float));
        GL.VertexArrayAttribFormat(_vao, 0, 3, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribFormat(_vao, 1, 2, VertexAttribType.Float, false, 3 * sizeof(float));
        GL.VertexArrayAttribBinding(_vao, 0, 0);
        GL.VertexArrayAttribBinding(_vao, 1, 0);
        GL.EnableVertexArrayAttrib(_vao, 0);
        GL.EnableVertexArrayAttrib(_vao, 1);
        
        GL.CreateRenderbuffers(1, out _msColorRenderBuffer);
        GL.NamedRenderbufferStorageMultisample(_msColorRenderBuffer, 4, RenderbufferStorage.Rgba16f, width, height);
        GL.CreateRenderbuffers(1, out _msDepthRenderBuffer);
        GL.NamedRenderbufferStorageMultisample(_msDepthRenderBuffer, 4, RenderbufferStorage.DepthComponent32, width, height);
        GL.NamedFramebufferRenderbuffer(_msFbo, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, _msColorRenderBuffer);
        GL.NamedFramebufferRenderbuffer(_msFbo, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _msDepthRenderBuffer);
        GL.CreateTextures(TextureTarget.Texture2D, 1, out _frameBufferColorTexture);
        GL.TextureStorage2D(_frameBufferColorTexture, 1, SizedInternalFormat.Rgba16f, width, height);
        GL.CreateTextures(TextureTarget.Texture2D, 1, out _frameBufferDepthTexture);
        GL.TextureStorage2D(_frameBufferDepthTexture, 1, SizedInternalFormat.DepthComponent32, width, height);
        GL.NamedFramebufferTexture(_fbo, FramebufferAttachment.ColorAttachment0, _frameBufferColorTexture, 0);
        GL.NamedFramebufferTexture(_fbo, FramebufferAttachment.DepthAttachment, _frameBufferDepthTexture, 0);
    }
    
    private void InitFog()
    {
        GL.CreateBuffers(1, out _fogsBuffer);
        float[] fogs =
        {
            14.0f / 255.0f, 11.0f / 255.0f, 10.0f / 255.0f, 1.0f, 0.01f, 0.0f, 0.0f, 0.0f, 254.0f / 255.0f,
            251.0f / 255.0f, 250.0F / 255.0f, 1.0f, 0.001f, 0.0f, 0.0f, 0.0f
        };

        GL.NamedBufferStorage(_fogsBuffer, fogs.Length * sizeof(float), fogs, BufferStorageFlags.None);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, _fogsBuffer);
    }
    
    private void InitWorldInfo()
    {
        GL.CreateBuffers(1, out _worldInfoBuffer);
        uint[] worldInfo =
        {
            EngineDefaults.ChunkWidth, EngineDefaults.ChunkHeight, EngineDefaults.ChunkDepth, (uint)MinecraftLibrary.Engine.World.Instance.GetWorldTime()
        };

        GL.NamedBufferStorage(_worldInfoBuffer, worldInfo.Length * sizeof(uint), worldInfo, BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 6, _worldInfoBuffer);
        _worldInfoPointer = GL.MapNamedBufferRange(_worldInfoBuffer, 3 * sizeof(uint), sizeof(uint),
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
    }

    private void InitCameraInfo()
    {
        GL.CreateBuffers(1, out _cameraInfoBuffer);
        Matrix4[] cameraInfo =
        {
            Camera.GetInstance().GetViewMatrix(), Camera.GetInstance().GetProjectionMatrix()
        };
        GL.NamedBufferStorage(_cameraInfoBuffer, cameraInfo.Length * Marshal.SizeOf<Matrix4>(), cameraInfo,
            BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapCoherentBit);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 5, _cameraInfoBuffer);
        _cameraInfoPointer = GL.MapNamedBufferRange(_cameraInfoBuffer, 0, 2 * Marshal.SizeOf<Matrix4>(),
            BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit | BufferAccessMask.MapCoherentBit);
    }

    internal WorldRenderer(PlayerRenderer playerRenderer, int width, int height)
    {
        if (Instance != null) throw new Exception("WorldRenderer already initialized");
        Instance = this;
        PlayerRenderer = playerRenderer;
        var world = MinecraftLibrary.Engine.World.Instance;
        _chunkTessellator = new ChunkTessellator(MinecraftLibrary.Engine.World.GetMaxChunksCount());
        for (var i = 0; i < _chunkRenderers.GetLength(0); i++)
        for (var j = 0; j < _chunkRenderers.GetLength(1); j++)
        for (var k = 0; k < _chunkRenderers.GetLength(2); k++)
        {
            var state = world.GetChunkAt(new Vector3i(i * EngineDefaults.ChunkWidth, j * EngineDefaults.ChunkHeight, k * EngineDefaults.ChunkDepth));
            _chunkRenderers[i, j, k] = new ChunkRenderer(state, (ushort)(i * _chunkRenderers.GetLength(2) * _chunkRenderers.GetLength(1) + j * _chunkRenderers.GetLength(2) + k));
            _chunkTessellator.SetChunkTransform(_chunkRenderers[i, j, k].ChunkId, _chunkRenderers[i, j, k].ModelMatrix);
        }
        world.OnPlayerAdded += OnPlayerAdded;
        InitFog();
        InitCameraInfo();
        InitWorldInfo();
        BlockRenderer.Setup();
        Texture.SetupTextures();
        GL.CreateVertexArrays(1, out _vao);
        GL.CreateBuffers(1, out _vbo);
        GL.CreateFramebuffers(1, out _fbo);
        GL.CreateFramebuffers(1, out _msFbo);
        InitWorldFrameBuffer(width, height);
    }

    ~WorldRenderer()
    {
        BlockRenderer.Terminate();
        Texture.Terminate();
        GL.DeleteBuffer(_fogsBuffer);
        GL.UnmapNamedBuffer(_worldInfoBuffer);
        GL.DeleteBuffer(_worldInfoBuffer);
        GL.UnmapNamedBuffer(_cameraInfoBuffer);
        GL.DeleteBuffer(_cameraInfoBuffer);
        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteTexture(_frameBufferDepthTexture);
        GL.DeleteTexture(_frameBufferColorTexture);
        GL.DeleteRenderbuffer(_msDepthRenderBuffer);
        GL.DeleteRenderbuffer(_msColorRenderBuffer);
        GL.DeleteFramebuffer(_msFbo);
        GL.DeleteFramebuffer(_fbo);
    }

    private void OnPlayerAdded(ushort entity)
    {
        OtherPlayers.Add(entity, new OtherPlayerRenderer(MinecraftLibrary.Engine.World.Instance.GetPlayer(entity).State));
    }

    internal void HandleWindowResize(int height, int width)
    {
        GL.Viewport(0, 0, width, height);
        _screenSize.X = width;
        _screenSize.Y = height;
        Camera.GetInstance().SetAspectRatio((float)width / height);
        GL.DeleteTexture(_frameBufferColorTexture);
        GL.DeleteRenderbuffer(_msDepthRenderBuffer);
        GL.DeleteRenderbuffer(_msColorRenderBuffer);
        GL.CreateRenderbuffers(1, out _msColorRenderBuffer);
        GL.NamedRenderbufferStorageMultisample(_msColorRenderBuffer, 4, RenderbufferStorage.Rgba16f, width, height);
        GL.CreateRenderbuffers(1, out _msDepthRenderBuffer);
        GL.NamedRenderbufferStorageMultisample(_msDepthRenderBuffer, 4, RenderbufferStorage.DepthComponent32, width, height);
        GL.NamedFramebufferRenderbuffer(_msFbo, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, _msColorRenderBuffer);
        GL.NamedFramebufferRenderbuffer(_msFbo, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _msDepthRenderBuffer);
        GL.CreateTextures(TextureTarget.Texture2D, 1, out _frameBufferColorTexture);
        GL.TextureStorage2D(_frameBufferColorTexture, 1, SizedInternalFormat.Rgba16f, width, height);
        GL.CreateTextures(TextureTarget.Texture2D, 1, out _frameBufferDepthTexture);
        GL.TextureStorage2D(_frameBufferDepthTexture, 1, SizedInternalFormat.DepthComponent32, width, height);
        GL.NamedFramebufferTexture(_fbo, FramebufferAttachment.ColorAttachment0, _frameBufferColorTexture, 0);
        GL.NamedFramebufferTexture(_fbo, FramebufferAttachment.DepthAttachment, _frameBufferDepthTexture, 0);
    }

    internal void Render(float delta)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _msFbo);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        PlayerRenderer.UpdateCamera(delta);
        var camera = Camera.GetInstance();
        Buffer.BlockCopy(camera.GetViewMatrixBytes(), 0, _matrixBuffer, 0, Marshal.SizeOf<Matrix4>());
        Buffer.BlockCopy(camera.GetProjectionMatrixBytes(), 0, _matrixBuffer, Marshal.SizeOf<Matrix4>(), Marshal.SizeOf<Matrix4>());
        Marshal.Copy(_matrixBuffer, 0, _cameraInfoPointer, _matrixBuffer.Length);
        var world = MinecraftLibrary.Engine.World.Instance;
        Marshal.Copy(EngineDefaults.GetBytes((uint)world.GetWorldTime()).ToArray(), 0, _worldInfoPointer, sizeof(uint));
        UpdateDirtyChunks();
        RenderChunks();
        GL.Enable(EnableCap.Blend);
        PlayerRenderer.RenderSelectionHighlight();
        GL.Disable(EnableCap.Blend);
        RenderPostProcessingEffects();
    }

    private void RenderPostProcessingEffects()
    {
        Shader.PostFxShader.Use();
        GL.BlitNamedFramebuffer(_msFbo, _fbo, 0, 0, _screenSize.X, _screenSize.Y, 0, 0, _screenSize.X, _screenSize.Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
        GL.BlitNamedFramebuffer(_msFbo, _fbo, 0, 0, _screenSize.X, _screenSize.Y, 0, 0, _screenSize.X, _screenSize.Y, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
        GL.Disable(EnableCap.DepthTest);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _frameBufferColorTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _frameBufferDepthTexture);
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    private void UpdateDirtyChunks()
    {
        byte counter = 0;
        for (var i = 0; i < _chunkRenderers.GetLength(0); i++)
        for (var j = 0; j < _chunkRenderers.GetLength(1); j++)
        for (var k = 0; k < _chunkRenderers.GetLength(2); k++)
        {
            var chunkRenderer = _chunkRenderers[i, j, k];
            if (!chunkRenderer.IsDirty()) continue;
            chunkRenderer.UpdateRenderer(_chunkTessellator);
            if (++counter == 8) return;
        }
    }

    private void RenderChunks()
    {
        _chunkTessellator.PrepareToDraw();
        _visibleChunkRenderers.Clear();
        var camera = Camera.GetInstance();
        var cameraFrustum = camera.GetFrustum();
        for (var i = 0; i < _chunkRenderers.GetLength(0); i++)
        for (var j = 0; j < _chunkRenderers.GetLength(1); j++)
        for (var k = 0; k < _chunkRenderers.GetLength(2); k++)
        {
            var chunkRenderer = _chunkRenderers[i, j, k];
            if (!cameraFrustum.CubeInFrustum(chunkRenderer.GetBoundingBox())) continue;
            _visibleChunkRenderers.Add(chunkRenderer);
        }

        var drawCount = Enum.GetValues<DrawType>().Length;
        for (var i = 0; i < drawCount; i++)
            foreach (var render in _visibleChunkRenderers)
                _chunkTessellator.AppendDrawCommand((DrawType)i, render.GetDrawCommand((DrawType)i));
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

    internal void ApplyTickChanges(Packet changePacket)
    {
        changePacket.ResetRead();
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
            if (playerId == PlayerRenderer.GetPlayerId())
                PlayerRenderer.ApplyRevertChanges(changePacket);
            else
                OtherPlayers[playerId].ApplyRevertChanges(changePacket);
        }
    }

    private void ApplyTickLightChanges(Packet changePacket)
    {
        changePacket.Read(out ushort lightCount);
        for (var i = 0; i < lightCount; i++)
        {
            changePacket.Read(out ushort x);
            changePacket.Read(out ushort z);
            changePacket.Read(out byte _);
            for (var j = 0; j < RenderHeight; j += EngineDefaults.ChunkHeight)
            {
                var chunkRenderer = GetChunkRendererAt(new Vector3i(x, j, z));
                chunkRenderer.LightUpdateColumn(new Vector2i(x, z));
                chunkRenderer = GetChunkRendererAt(new Vector3i(x + 1, j, z));
                chunkRenderer.LightUpdateColumn(new Vector2i(x + 1, z));
                chunkRenderer = GetChunkRendererAt(new Vector3i(x - 1, j, z));
                chunkRenderer.LightUpdateColumn(new Vector2i(x - 1, z));
                chunkRenderer = GetChunkRendererAt(new Vector3i(x, j, z + 1));
                chunkRenderer.LightUpdateColumn(new Vector2i(x, z + 1));
                chunkRenderer = GetChunkRendererAt(new Vector3i(x, j, z - 1));
                chunkRenderer.LightUpdateColumn(new Vector2i(x, z - 1));
            }
        }
    }

    internal bool IsOutOfBounds(Vector3i pos)
    {
        return pos.X < BaseVector.X || pos.Y < BaseVector.Y || pos.Z < BaseVector.Z || pos.X >= RenderWidth || pos.Y >= RenderHeight ||
               pos.Z >= RenderDepth;
    }

    internal ChunkRenderer GetChunkRendererAt(Vector3i position)
    {
        var helper = position - BaseVector;
        return _chunkRenderers[helper.X / EngineDefaults.ChunkWidth, helper.Y / EngineDefaults.ChunkHeight, helper.Z / EngineDefaults.ChunkDepth];
    }
}