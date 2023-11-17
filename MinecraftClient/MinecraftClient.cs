using MinecraftClient.Render.World;
using MinecraftLibrary.Engine;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
            Flags = ContextFlags.Default, StartVisible = true, StartFocused = true
        })
    {
        World = world;
        WorldRenderer = new WorldRenderer();
        GLFW.SetErrorCallback(HandleError);
        CursorState = CursorState.Grabbed;
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
        base.OnRenderFrame(args);
    }

    private static void HandleError(ErrorCode errorCode, string description)
    {
        Console.WriteLine($"Error: {errorCode} - {description}");
    }
}