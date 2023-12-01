using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.Shaders;

public class Shader
{
    private Dictionary<string, int> Uniforms = new();
    public int Program { get; }
    
    public static Shader MainShader { get; } = new("Render/Shaders/VertexShader.glsl", "Render/Shaders/FragmentShader.glsl", "Render/Shaders/GeometryShader.glsl");

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
            Uniforms.Add(name, GL.GetUniformLocation(Program, name));
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
            Uniforms.Add(name, GL.GetUniformLocation(Program, name));
        }
    }
    
    public void Use()
    {
        GL.UseProgram(Program);
    }
    
    public int GetUniformBlockIndex(string name)
    {
        return GL.GetUniformBlockIndex(Program, name);
    }
    
    public void SetInt(string name, int value)
    {
        GL.Uniform1(Uniforms[name], value);
    }
    
    public void SetUnsignedInt(string name, uint value)
    {
        GL.Uniform1(Uniforms[name], value);
    }
    
    public void SetFloat(string name, float value)
    {
        GL.Uniform1(Uniforms[name], value);
    }
    
    public void SetVector3(string name, Vector3 value)
    {
        GL.Uniform3(Uniforms[name], value);
    }
    
    public void SetMat4(string name, Matrix4 value)
    {
        GL.UniformMatrix4(Uniforms[name], false, ref value);
    }
}