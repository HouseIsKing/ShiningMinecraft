#version 460 core
layout (location = 0) in uint BlockType16_Brightness6;

struct Block
{
    vec4 bounds[2];
    uvec4 indexTextures_Colors_SpecialEffects[6];
};

struct Fog
{
    vec4 fogColor;
    float fogDensity;
};

layout(binding = 1) uniform fogBlock { Fog fogs[2]; };
layout(binding = 2) restrict readonly buffer blocksBuffer { Block blocks[8]; };
layout(binding = 4) restrict readonly buffer transformationMatriciesBuffer { mat4 transformationMatricies[2048]; };
uniform mat4 view;
uniform uint chunkWidth;
uniform uint chunkHeight;
uniform uint chunkDepth;

out vec3 boundsMin;
out vec3 boundsMax;
out flat uint textureIndex;
out uint Face;
out uint specialEffects;
out float fogDensity;
out vec4 color;
out vec4 fogColor;
out mat4 transformationMatrix;

void main()
{
    uint BlockType = BlockType16_Brightness6 >> 16;
    uint BrightnessBits = (BlockType16_Brightness6 >> 10) & 0x3F;
    uint indexInChunk = gl_VertexID % (chunkWidth * chunkDepth * chunkHeight);
    gl_Position = vec4((indexInChunk / chunkHeight) / chunkDepth, (indexInChunk / chunkDepth) % chunkHeight, indexInChunk % chunkDepth, 1.0f);
    Block block = blocks[BlockType];
    Face = (gl_DrawID % 3) * 2;
    boundsMin = block.bounds[0].xyz;
    boundsMax = block.bounds[1].xyz;
    transformationMatrix = transformationMatricies[gl_DrawID / 3];
    vec3 boundsPos = (transformationMatrix * (vec4(boundsMax, 0.0f) + gl_Position)).xyz;
    vec3 camPos = inverse(view)[3].xyz;
    if(boundsPos[(Face + 1) % 3] > camPos[(Face + 1) % 3])
    {
        Face += 1;
    }
    uint brightness = (BrightnessBits >> Face) & 0x1;
    fogDensity = fogs[brightness].fogDensity;
    fogColor = fogs[brightness].fogColor;
    specialEffects = block.indexTextures_Colors_SpecialEffects[Face].z;
	textureIndex = block.indexTextures_Colors_SpecialEffects[Face].x;
    color = vec4((block.indexTextures_Colors_SpecialEffects[Face].y >> 24) / 255.0F, (block.indexTextures_Colors_SpecialEffects[Face].y >> 16 & 0xFF) / 255.0F, (block.indexTextures_Colors_SpecialEffects[Face].y >> 8 & 0xFF) / 255.0F, (block.indexTextures_Colors_SpecialEffects[Face].y & 0xFF) / 255.0F);
}