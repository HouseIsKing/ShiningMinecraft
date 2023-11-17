using OpenTK.Mathematics;

namespace MinecraftClient.Render;

public struct Vertex
{
    public Vector3 Pos;
    public Vector2 Uv;
    public Vector4 Color;
    public ushort IndexTexture;
    public byte Brightness;
    public byte SpecialFactors;
}