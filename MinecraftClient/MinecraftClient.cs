using System.Diagnostics;
using System.Runtime.InteropServices;
using MinecraftClient.Render.Entities.Player;
using MinecraftClient.Render.World;
using MinecraftLibrary.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode;

namespace MinecraftClient;

public abstract class MinecraftClient : GameWindow 
{
    private World World { get; }
    private WorldRenderer WorldRenderer { get; }
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
        WorldRenderer = new WorldRenderer();
        GL.DebugMessageCallback(HandleDebug, IntPtr.Zero);
        GLFW.SetErrorCallback(HandleError);
        CursorState = CursorState.Grabbed;
        GL.ClearColor(0.5f, 0.8f, 1.0f, 1.0f);
        GL.ClearDepth(1.0f);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        World.GenerateLevel();
        Console.WriteLine("Done loading world");
    }

    private static void HandleDebug(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userparam)
    {
        if (severity == DebugSeverity.DebugSeverityNotification) return;
        Console.WriteLine($"[{severity}] {Marshal.PtrToStringAnsi(message, length)}");
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        //World.HandleWindowResize(e.Width, e.Height);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        var start = Stopwatch.GetTimestamp();
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        WorldRenderer.Render();
        Camera.GetInstance().Yaw += MouseState.Delta.X;
        Camera.GetInstance().Pitch -= MouseState.Delta.Y;
        Camera.GetInstance().Position += Camera.GetInstance().GetFrontVector() * Convert.ToSingle(KeyboardState.IsKeyDown(Keys.W)) * 0.005f;
        //Console.WriteLine($"Yaw: {Camera.GetInstance().Yaw} Pitch: {Camera.GetInstance().Pitch}");
        SwapBuffers();
        GL.Finish();
        var timeTook = (float)Stopwatch.GetElapsedTime(start).TotalSeconds;
        Console.WriteLine($"Render took {timeTook} seconds");
    }

    private static void HandleError(ErrorCode errorCode, string description)
    {
        Console.WriteLine($"Error: {errorCode} - {description}");
    }
}