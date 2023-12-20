#version 460 core
layout (location = 0) in uint BlockType16_Brightness6;

layout(binding = 6) uniform WorldInfo { uint chunkWidth; uint chunkHeight; uint chunkDepth; uint worldTime; };

out uint BlockType16_Brightness6a;
out uint chunkIndex;

void main()
{
    BlockType16_Brightness6a = BlockType16_Brightness6;
    chunkIndex = gl_VertexID / (chunkWidth * chunkDepth * chunkHeight);
    uint indexInChunk = gl_VertexID % (chunkWidth * chunkDepth * chunkHeight);
    gl_Position = vec4((indexInChunk / chunkHeight) / chunkDepth, (indexInChunk / chunkDepth) % chunkHeight, indexInChunk % chunkDepth, 1.0f);
}