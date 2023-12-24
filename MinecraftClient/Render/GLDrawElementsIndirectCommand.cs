namespace MinecraftClient.Render;

public struct GlDrawElementsIndirectCommand
{ 
    public uint Count = 0;
    public uint InstanceCount = 1;
    public uint FirstIndex = 0;
    public uint BaseVertex = 0;
    public uint BaseInstance = 0;

    public GlDrawElementsIndirectCommand()
    {
    }
}