using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.Shaders;

internal sealed class Shader
{
    private readonly Dictionary<string, int> _uniforms = new();
    public int Program { get; }

    internal static Shader NormalBlockShader { get; } = new("Render/Shaders/BlockShaders/NormalBlockVS.glsl", "Render/Shaders/BlockShaders/NormalBlockFS.glsl", "Render/Shaders/BlockShaders/NormalBlockGS.glsl");
    internal static Shader CrossBlockShader { get; } = new("Render/Shaders/BlockShaders/CrossBlockVS.glsl", "Render/Shaders/BlockShaders/CrossBlockFS.glsl", "Render/Shaders/BlockShaders/CrossBlockGS.glsl");
    internal static Shader SelectionHighlightAShader { get; } = new("Render/Shaders/MiscShaders/SelectionHighlightVS.glsl", "Render/Shaders/MiscShaders/SelectionHighlightFS.glsl", "Render/Shaders/MiscShaders/SelectionHighlightGS.glsl");
    internal static Shader SelectionHighlightBShader { get; } = new("Render/Shaders/MiscShaders/SelectionHighlightModeVS.glsl", "Render/Shaders/MiscShaders/SelectionHighlightModeFS.glsl", "Render/Shaders/MiscShaders/SelectionHighlightModeGS.glsl");
    
    internal static Shader PostFxShader { get; } = new("Render/Shaders/MiscShaders/PostEffectsVS.glsl", "Render/Shaders/MiscShaders/PostEffectsFS.glsl");

    private static void CompileShader(int shader)
    {
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
        if (success != 0) return;

        Console.WriteLine("ERROR COMPILATION FAILED:" + GL.GetShaderInfoLog(shader));
    }

    private static void LinkProgram(int program)
    {
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var success);
        if (success != 0) return;

        Console.WriteLine("ERROR LINKING FAILED:" + GL.GetProgramInfoLog(program));
    }

    ~Shader()
    {
        GL.DeleteProgram(Program);
    }

    private Shader(string vertexPath, string fragmentPath)
    {
        var vertexCode = File.ReadAllText(vertexPath);
        var fragmentCode = File.ReadAllText(fragmentPath);
        var vertShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertShader, vertexCode);
        CompileShader(vertShader);
        var fragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragShader, fragmentCode);
        CompileShader(fragShader);
        Program = GL.CreateProgram();
        GL.AttachShader(Program, vertShader);
        GL.AttachShader(Program, fragShader);
        LinkProgram(Program);
        GL.DetachShader(Program, vertShader);
        GL.DetachShader(Program, fragShader);
        GL.DeleteShader(vertShader);
        GL.DeleteShader(fragShader);
        GL.GetProgram(Program, GetProgramParameterName.ActiveUniforms, out var uniformCount);
        for (var i = 0; i < uniformCount; i++)
        {
            var name = GL.GetActiveUniform(Program, i, out _, out _);
            _uniforms.Add(name, GL.GetUniformLocation(Program, name));
        }
    }

    private Shader(string vertexPath, string fragmentPath, string geometryPath)
    {
        var vertexCode = File.ReadAllText(vertexPath);
        var fragmentCode = File.ReadAllText(fragmentPath);
        var geometryCode = File.ReadAllText(geometryPath);
        var vertShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertShader, vertexCode);
        CompileShader(vertShader);
        var fragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragShader, fragmentCode);
        CompileShader(fragShader);
        var geomShader = GL.CreateShader(ShaderType.GeometryShader);
        GL.ShaderSource(geomShader, geometryCode);
        CompileShader(geomShader);
        Program = GL.CreateProgram();
        GL.AttachShader(Program, vertShader);
        GL.AttachShader(Program, fragShader);
        GL.AttachShader(Program, geomShader);
        LinkProgram(Program);
        GL.DetachShader(Program, vertShader);
        GL.DetachShader(Program, fragShader);
        GL.DetachShader(Program, geomShader);
        GL.DeleteShader(vertShader);
        GL.DeleteShader(fragShader);
        GL.DeleteShader(geomShader);
        GL.GetProgram(Program, GetProgramParameterName.ActiveUniforms, out var uniformCount);
        for (var i = 0; i < uniformCount; i++)
        {
            var name = GL.GetActiveUniform(Program, i, out _, out _);
            _uniforms.Add(name, GL.GetUniformLocation(Program, name));
        }
    }

    internal void Use()
    {
        GL.UseProgram(Program);
    }
    
    public int GetUniformBlockIndex(string name)
    {
        return GL.GetUniformBlockIndex(Program, name);
    }
    
    public void SetInt(string name, int value)
    {
        GL.Uniform1(_uniforms[name], value);
    }
    
    public void SetUnsignedInt(string name, uint value)
    {
        GL.Uniform1(_uniforms[name], value);
    }

    internal void SetFloat(string name, float value)
    {
        GL.Uniform1(_uniforms[name], value);
    }

    internal void SetVector3(string name, Vector3 value)
    {
        GL.Uniform3(_uniforms[name], value);
    }
    
    public void SetMat4(string name, Matrix4 value)
    {
        GL.UniformMatrix4(_uniforms[name], false, ref value);
    }
}