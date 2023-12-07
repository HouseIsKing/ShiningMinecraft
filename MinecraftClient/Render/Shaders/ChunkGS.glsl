#version 460 core
layout (points) in;
layout (triangle_strip, max_vertices = 24) out;

struct Fog
{
    vec4 fogColor;
    float fogDensity;
};

struct Block
{
    vec4 bounds[2];
    uvec4 indexTextures_Colors[6];
};

layout(binding = 2) restrict readonly buffer blocksBuffer { Block blocks[8]; };
layout(binding = 1) uniform fogBlock { Fog fogs[2]; };
layout(binding = 5) uniform CameraInfo { mat4 view; mat4 projection; };
layout(binding = 4) restrict readonly buffer transformationMatricesBuffer { mat4 transformationMatrices[2048]; };

in uint BlockType16_Brightness6a[];
in uint chunkIndex[];

out vec4 fragColor;
out vec2 fragTexture;
out flat uint fragTextureIndex;

void ApplyFog(uint brightness)
{
    float distance = length(gl_Position);
    float fogFactor = 1.0F - exp(-fogs[brightness].fogDensity * distance);
    fragColor *= (0.6f + 0.4f * brightness);
    fragColor = mix(fragColor, fogs[brightness].fogColor, fogFactor);
    gl_Position = projection * gl_Position;
}

void NextVertex(uint index, uint brightness)
{
    ApplyFog(brightness);
    fragTexture = vec2(index % 2, index / 2);
    EmitVertex();
}

void main()
{
    uint brightnessBits = (BlockType16_Brightness6a[0] >> 10) & 0x3F;
    uint blockType = BlockType16_Brightness6a[0] >> 16;
    vec4 boundsMin = blocks[blockType].bounds[0];
    vec4 boundsMax = blocks[blockType].bounds[1];
    vec4 pos = gl_in[0].gl_Position;
    vec4 finalPos = transformationMatrices[chunkIndex[0]] * pos;
    vec4 camPos = inverse(view)[3];
        
    /*if (specialEffects[0] == 1u)
    {
        float speed = 2.0F;
        float pi = 3.14159265359F;
        float magnitude = (sin((pos.y + pos.x + worldTime * pi / ((28.0F) * speed))) * 0.15F + 0.15F) * 0.20F;
        float d0 = sin(worldTime * pi / (112.0F * speed)) * 3.0F - 1.5F;
        float d1 = sin(worldTime * pi / (142.0F * speed)) * 3.0F - 1.5F;
        float d2 = sin(worldTime * pi / (132.0F * speed)) * 3.0F - 1.5F;
        float d3 = sin(worldTime * pi / (122.0F * speed)) * 3.0F - 1.5F;
        pos.x += sin((worldTime * pi / (18.0F * speed)) + (-pos.x + d0) * 1.6F + (pos.z + d1) * 1.6F) * magnitude;
        pos.z += sin((worldTime * pi / (17.0F * speed)) + (pos.z + d2) * 1.6F + (-pos.x + d3) * 1.6F) * magnitude;
        pos.y += sin((worldTime * pi / (11.0F * speed)) + (pos.z + d2) + (pos.x + d3)) * (magnitude / 2.0F);
    }*/
    uint brightness = 0;
    uint c = 0;
    vec4 basicColor = vec4(0.0F, 0.0F, 0.0F, 0.0F);
    if (finalPos.y > camPos.y)
    {
        brightness = (brightnessBits >> 1) & 0x1;
        fragTextureIndex = blocks[blockType].indexTextures_Colors[1].x;
        c = blocks[blockType].indexTextures_Colors[1].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else
    {
        brightness = brightnessBits & 0x1;
        fragTextureIndex = blocks[blockType].indexTextures_Colors[0].x;
        c = blocks[blockType].indexTextures_Colors[0].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    if (finalPos.x > camPos.x)
    {
        brightness = (brightnessBits >> 3) & 0x1;
        fragTextureIndex = blocks[blockType].indexTextures_Colors[3].x;
        c = blocks[blockType].indexTextures_Colors[3].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else
    {
        brightness = (brightnessBits >> 2) & 0x1;
        fragTextureIndex = blocks[blockType].indexTextures_Colors[2].x;
        c = blocks[blockType].indexTextures_Colors[2].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    if (finalPos.z > camPos.z)
    {
        brightness = brightnessBits >> 5;
        fragTextureIndex = blocks[blockType].indexTextures_Colors[5].x;
        c = blocks[blockType].indexTextures_Colors[5].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else
    {
        brightness = (brightnessBits >> 4) & 0x1;
        fragTextureIndex = blocks[blockType].indexTextures_Colors[4].x;
        c = blocks[blockType].indexTextures_Colors[4].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrices[chunkIndex[0]] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
}