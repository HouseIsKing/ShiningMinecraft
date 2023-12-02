#version 460 core
in layout(location = 0) uint BlockType16_Brightness;

struct Block
{
    vec4 bounds[2];
    uvec4 indexTextures_Colors[6];
};

struct Fog
{
    vec4 fogColor;
    float fogDensity;
};

layout(binding = 1) uniform fogBlock { Fog fogs[2]; };
layout(binding = 2) restrict readonly buffer blocksBuffer { Block blocks[8]; };
uniform float alpha;

out float fogDensity;
out vec4 color;
out vec4 fogColor;
out vec3 boundsMin;
out vec3 boundsMax;

void main()
{
    uint BlockType = BlockType16_Brightness >> 16;
    uint BrightnessBit = (BlockType16_Brightness >> 15) & 0x1;
    color = vec4(alpha) * (0.6f + 0.4f * BrightnessBit);
    fogDensity = fogs[BrightnessBit].fogDensity;
    fogColor = fogs[BrightnessBit].fogColor;
    Block block = blocks[BlockType];
    boundsMin = block.bounds[0].xyz;
    boundsMax = block.bounds[1].xyz;
}