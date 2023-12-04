#version 460 core
layout (location = 0) in uint BlockType16_Brightness6;

struct Block
{
    vec4 bounds[2];
    uvec4 indexTextures_Colors[6];
};

layout(binding = 2) restrict readonly buffer blocksBuffer { Block blocks[8]; };
layout(binding = 6) uniform WorldInfo { uint chunkWidth; uint chunkHeight; uint chunkDepth; uint worldTime; };
layout(binding = 4) restrict readonly buffer transformationMatriciesBuffer { mat4 transformationMatricies[2048]; };

out uint blockType;
out uint brightnessBits;
out mat4 transformationMatrix;

void main()
{
    blockType = BlockType16_Brightness6 >> 16;
    brightnessBits = (BlockType16_Brightness6 >> 10) & 0x3F;
    transformationMatrix = transformationMatricies[gl_DrawID];
    uint indexInChunk = gl_VertexID % (chunkWidth * chunkDepth * chunkHeight);
    gl_Position = vec4((indexInChunk / chunkHeight) / chunkDepth, (indexInChunk / chunkDepth) % chunkHeight, indexInChunk % chunkDepth, 1.0f);
}