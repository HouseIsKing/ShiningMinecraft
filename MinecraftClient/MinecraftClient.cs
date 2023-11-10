using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MinecraftClient;

public class MinecraftClient : GameWindow
{
    protected MinecraftClient() : base(GameWindowSettings.Default,
        new NativeWindowSettings
        {
            API = ContextAPI.OpenGL, APIVersion = new Version(4, 6), Vsync = VSyncMode.Off,
            Size = new Vector2i(1280, 720), Title = "Shining Minecraft Client", NumberOfSamples = 4,
            Profile = ContextProfile.Core, MinimumSize = new Vector2i(640, 480), WindowState = WindowState.Normal,
            WindowBorder = WindowBorder.Resizable, CurrentMonitor = Monitors.GetPrimaryMonitor().Handle,
            Flags = ContextFlags.Default, StartVisible = true, StartFocused = true
        })
    {
        CursorState = CursorState.Grabbed;
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }
}