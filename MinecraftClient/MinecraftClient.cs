﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using MinecraftClient.Render.Entities.Player;
using MinecraftClient.Render.World;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Input;
using MinecraftLibrary.Network;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode;

namespace MinecraftClient;

public abstract class MinecraftClient : GameWindow 
{
    protected World World { get; }
    protected readonly WorldRenderer WorldRenderer;
    protected PlayerState Player;
    protected ClientInput Input = new();
    protected double Ticker;
    private long _startTickTime;
    protected uint InputId { get; private set; }
    protected readonly Packet[] PacketHistory = new Packet[EngineDefaults.PacketHistorySize];
    protected MinecraftClient(World world) : base(GameWindowSettings.Default,
        new NativeWindowSettings
        {
            API = ContextAPI.OpenGL, APIVersion = new Version(4, 6), Vsync = VSyncMode.Off,
            Size = new Vector2i(1280, 720), Title = "Shining Minecraft Client", NumberOfSamples = 4,
            Profile = ContextProfile.Core, MinimumSize = new Vector2i(640, 480), WindowState = WindowState.Normal,
            WindowBorder = WindowBorder.Resizable, CurrentMonitor = Monitors.GetPrimaryMonitor().Handle,
            Flags = ContextFlags.Debug, StartVisible = true, StartFocused = true
        })
    {
        for (var i = 0; i < PacketHistory.Length; i++) PacketHistory[i] = new Packet(new PacketHeader(PacketType.WorldChange));
        World = world;
        World.OnTickStart += PreTick;
        World.OnTickEnd += PostTick;
        WorldRenderer = new WorldRenderer(new PlayerRenderer(null), 1280, 720);
        GL.DebugMessageCallback(HandleDebug, IntPtr.Zero);
        GLFW.SetErrorCallback(HandleError);
        CursorState = CursorState.Grabbed;
        GL.ClearColor(0.5f, 0.8f, 1.0f, 1.0f);
        GL.ClearDepth(1.1f);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Multisample);
        //GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.CullFace);
    }

    private static void HandleDebug(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
    {
        if (severity == DebugSeverity.DebugSeverityNotification) return;
        Console.WriteLine($"[{severity}] {Marshal.PtrToStringAnsi(message, length)}");
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        WorldRenderer.HandleWindowResize(e.Height, e.Width);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (_startTickTime == 0) _startTickTime = Stopwatch.GetTimestamp();
        base.OnUpdateFrame(args);
        Input.MouseX += MouseState.Delta.X;
        Input.MouseY -= MouseState.Delta.Y;
        Input.KeySet1 = (byte)(Convert.ToInt32(MouseState.IsButtonDown(MouseButton.Button1)) |
                               (Convert.ToInt32(MouseState.IsButtonDown(MouseButton.Button2)) << 1) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.Space)) << 2) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.R)) << 3) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.G)) << 4) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.D1)) << 5) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.D2)) << 6) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.D3)) << 7));
        Input.KeySet2 = (byte)(Convert.ToInt32(KeyboardState.IsKeyDown(Keys.D4)) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.D5)) << 1) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.W)) << 2) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.S)) << 3) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.A)) << 4) |
                               (Convert.ToInt32(KeyboardState.IsKeyDown(Keys.D)) << 5));
        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
        var i = (int)(Ticker / EngineDefaults.TickRate);
        for (; i > 0; i--)
        {
            var startTick = Stopwatch.GetTimestamp();
            var p = PacketHistory[(InputId + 1ul) % (ulong)PacketHistory.Length];
            World.Tick(p, true);
            WorldRenderer.ApplyTickChanges(p);
            Ticker -= EngineDefaults.TickRate;
            var timeTookTick = (float)Stopwatch.GetElapsedTime(startTick).TotalMilliseconds;
            Console.WriteLine($"Tick took {timeTookTick} ms");
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        WorldRenderer.Render((float)Ticker / EngineDefaults.TickRate);
        SwapBuffers();
        var timeTook = Stopwatch.GetElapsedTime(_startTickTime).TotalMilliseconds;
        if (timeTook > 5) Console.WriteLine($"Render took {timeTook} ms");
        var timer = Stopwatch.GetTimestamp();
        Ticker += new TimeSpan(timer - _startTickTime).TotalSeconds;
        _startTickTime = timer;
    }

    private static void HandleError(ErrorCode errorCode, string description)
    {
        Console.WriteLine($"Error: {errorCode} - {description}");
    }

    protected virtual void PreTick()
    {
        if (!Player.DidSpawn) return;
        Input.MouseX *= 0.4f;
        Input.MouseY *= 0.4f;
        InputId++;
        World.Instance.GetPlayer(Player.EntityId).AddInput(InputId, Input);
    }

    protected virtual void PostTick()
    {
        Input = new ClientInput();
    }
}