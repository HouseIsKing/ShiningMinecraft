using System.Diagnostics;
using System.Runtime.InteropServices;
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
    protected WorldRenderer WorldRenderer;
    protected PlayerState Player;
    protected ClientInput Input = new();
    private float _ticker;
    protected readonly Packet Packet = new(new PacketHeader());
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
        World = world;
        World.OnTickStart += PreTick;
        World.OnTickEnd += PostTick;
        GL.DebugMessageCallback(HandleDebug, IntPtr.Zero);
        GLFW.SetErrorCallback(HandleError);
        CursorState = CursorState.Grabbed;
        GL.ClearColor(0.5f, 0.8f, 1.0f, 1.0f);
        GL.ClearDepth(1.0f);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
    }

    private static void HandleDebug(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userparam)
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
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        var start = Stopwatch.GetTimestamp();
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        var i = (int)(_ticker / EngineDefaults.TickRate);
        for (; i > 0; i--)
        {
            Packet.Reset();
            World.Tick(Packet, true);
            WorldRenderer.ApplyTickChanges(Packet);
            _ticker -= EngineDefaults.TickRate;
        }
        WorldRenderer.Render(_ticker / EngineDefaults.TickRate);
        //Camera.GetInstance().Yaw += MouseState.Delta.X;
        //Camera.GetInstance().Pitch -= MouseState.Delta.Y;
        //Camera.GetInstance().Position += Camera.GetInstance().GetFrontVector() * Convert.ToSingle(KeyboardState.IsKeyDown(Keys.W)) * 0.02f;
        //Console.WriteLine($"Yaw: {Camera.GetInstance().Yaw} Pitch: {Camera.GetInstance().Pitch}");
        GL.Flush();
        SwapBuffers();
        var timeTook = (float)Stopwatch.GetElapsedTime(start).TotalSeconds;
        if (timeTook > 0.01f) Console.WriteLine($"Render took {timeTook} seconds");
        _ticker += timeTook;
    }

    private static void HandleError(ErrorCode errorCode, string description)
    {
        Console.WriteLine($"Error: {errorCode} - {description}");
    }

    protected abstract void PreTick();
    protected abstract void PostTick();
}