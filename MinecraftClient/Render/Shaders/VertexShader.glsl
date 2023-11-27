#version 460 core
layout (location = 0) in uint BlockType16_Brightness6;

struct Fog
{
    vec4 fogColor;
    float fogDensity;
};

struct Block
{
    vec4 positions[24];
    uvec4 indexTextures_Colors_SpecialEffects[6];
};

layout(binding = 1) uniform fogBlock { Fog fogs[2]; };
layout(binding = 2) restrict readonly buffer blocksBuffer { Block blocks[8]; };
uniform mat4 transformationMatrix;
uniform mat4 view;
uniform mat4 projection;
uniform uint worldTime;
uniform uint chunkHeight;
uniform uint chunkDepth;
out vec2 outTexture;
out vec4 color;
out flat uint textureIndex;

void main()
{
    uint BlockType = BlockType16_Brightness6 >> 16;
    uint BrightnessBits = (BlockType16_Brightness6 >> 10) & 0x3F;
    uint indexHelper = gl_VertexID % 4;
    uint indexInChunk = gl_VertexID / 4;
    uint Face = gl_DrawID;
    Block block = blocks[BlockType];
    vec3 helper = vec3((indexInChunk / chunkHeight) / chunkDepth, (indexInChunk / chunkDepth) % chunkHeight, indexInChunk % chunkDepth);
    uint brightness = (BrightnessBits >> Face) & 0x1;
    vec3 pos = helper + block.positions[indexHelper + (Face * 4)].xyz;
    if((block.indexTextures_Colors_SpecialEffects[Face].z & 0x1) == 1)
    {
        float speed = 2.0F;
        float pi = 3.14159265359F;
        float magnitude = (sin((helper.y + helper.x + worldTime * pi / ((28.0F) * speed))) * 0.15F + 0.15F) * 0.20F;
        float d0 = sin(worldTime * pi / (112.0F * speed)) * 3.0F - 1.5F;
        float d1 = sin(worldTime * pi / (142.0F * speed)) * 3.0F - 1.5F;
        float d2 = sin(worldTime * pi / (132.0F * speed)) * 3.0F - 1.5F;
        float d3 = sin(worldTime * pi / (122.0F * speed)) * 3.0F - 1.5F;
        pos.x += sin((worldTime * pi / (18.0F * speed)) + (-helper.x + d0) * 1.6F + (helper.z + d1) * 1.6F) * magnitude;
        pos.z += sin((worldTime * pi / (17.0F * speed)) + (helper.z + d2) * 1.6F + (-helper.x + d3) * 1.6F) * magnitude;
        pos.y += sin((worldTime * pi / (11.0F * speed)) + (helper.z + d2) + (helper.x + d3)) * (magnitude / 2.0F);
    }
    vec4 posRelativeToCamera = view * transformationMatrix * vec4(pos, 1.0);
	gl_Position = projection * posRelativeToCamera;
	outTexture = vec2(indexHelper % 2 , (indexHelper / 2) % 2);
	textureIndex = block.indexTextures_Colors_SpecialEffects[Face].x;
	float distance = length(posRelativeToCamera.xyz);
	vec4 fogColor = fogs[brightness].fogColor;
    float fogFactor = 1.0F - exp(-fogs[brightness].fogDensity * distance);
    vec4 colorVector = vec4((block.indexTextures_Colors_SpecialEffects[Face].y >> 24) / 255.0F, (block.indexTextures_Colors_SpecialEffects[Face].y >> 16 & 0xFF) / 255.0F, (block.indexTextures_Colors_SpecialEffects[Face].y >> 8 & 0xFF) / 255.0F, (block.indexTextures_Colors_SpecialEffects[Face].y & 0xFF) / 255.0F);
    color =  mix(colorVector, fogColor, fogFactor);
}