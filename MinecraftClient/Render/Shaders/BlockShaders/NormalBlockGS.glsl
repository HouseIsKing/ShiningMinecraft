#version 460 core
layout (points) in;
layout (triangle_strip, max_vertices = 12) out;

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
    Block block = blocks[blockType];
    vec4 boundsMin = block.bounds[0];
    vec4 boundsMax = block.bounds[1];
    vec4 minusPos = transformationMatrices[chunkIndex[0]] * (gl_in[0].gl_Position + boundsMin);
    vec4 plusPos = transformationMatrices[chunkIndex[0]] * (gl_in[0].gl_Position + boundsMax);
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
    if (minusPos.y > camPos.y)
    {
        brightness = (brightnessBits >> 1) & 0x1;
        fragTextureIndex = block.indexTextures_Colors[1].x;
        c = block.indexTextures_Colors[1].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, minusPos.y, minusPos.z, 1.0);
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, minusPos.y, minusPos.z, 1.0);
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, minusPos.y, plusPos.z, 1.0);
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, minusPos.y, plusPos.z, 1.0);
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else if (plusPos.y < camPos.y)
    {
        brightness = brightnessBits & 0x1;
        fragTextureIndex = block.indexTextures_Colors[0].x;
        c = block.indexTextures_Colors[0].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, plusPos.y, minusPos.z, 1.0);
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, plusPos.y, plusPos.z, 1.0);
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, plusPos.y, minusPos.z, 1.0);
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, plusPos.y, plusPos.z, 1.0);
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    if (minusPos.x > camPos.x)
    {
        brightness = (brightnessBits >> 3) & 0x1;
        fragTextureIndex = block.indexTextures_Colors[3].x;
        c = block.indexTextures_Colors[3].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, minusPos.y, minusPos.z, 1.0);
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, minusPos.y, plusPos.z, 1.0);
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, plusPos.y, minusPos.z, 1.0);
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, plusPos.y, plusPos.z, 1.0);
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else if (plusPos.x < camPos.x)
    {
        brightness = (brightnessBits >> 2) & 0x1;
        fragTextureIndex = block.indexTextures_Colors[2].x;
        c = block.indexTextures_Colors[2].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, minusPos.y, minusPos.z, 1.0);
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, plusPos.y, minusPos.z, 1.0);
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, minusPos.y, plusPos.z, 1.0);
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, plusPos.y, plusPos.z, 1.0);
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    if (minusPos.z > camPos.z)
    {
        brightness = brightnessBits >> 5;
        fragTextureIndex = block.indexTextures_Colors[5].x;
        c = block.indexTextures_Colors[5].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, minusPos.y, minusPos.z, 1.0);
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, plusPos.y, minusPos.z, 1.0);
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, minusPos.y, minusPos.z, 1.0);
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, plusPos.y, minusPos.z, 1.0);
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else if (plusPos.z < camPos.z)
    {
        brightness = (brightnessBits >> 4) & 0x1;
        fragTextureIndex = block.indexTextures_Colors[4].x;
        c = block.indexTextures_Colors[4].y;
        basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, minusPos.y, plusPos.z, 1.0);
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, minusPos.y, plusPos.z, 1.0);
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(minusPos.x, plusPos.y, plusPos.z, 1.0);
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * vec4(plusPos.x, plusPos.y, plusPos.z, 1.0);
        NextVertex(3u, brightness);
        EndPrimitive();
    }
}