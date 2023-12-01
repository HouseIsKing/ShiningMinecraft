namespace MinecraftClient.Render;

public struct GlDrawElementsIndirectCommand
{ 
    public uint count = 0;
    public uint instanceCount = 1;
    public uint firstIndex = 0;
    public uint baseVertex = 0;
    public uint baseInstance = 0;

    public GlDrawElementsIndirectCommand()
    {
    }
}