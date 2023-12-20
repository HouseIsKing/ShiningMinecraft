#version 460 core
layout(points) in;
layout(triangle_strip, max_vertices = 12) out;

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
layout(binding = 5) uniform CameraInfo { mat4 view; mat4 projection; };
uniform float alpha;
uniform float bright;
uniform vec3 hitPos;

in uint BlockType16_Brightness6a[];

out vec4 fragColor;
out vec2 fragTexture;
out flat uint fragTextureIndex;

void ApplyFog(uint brightness)
{
    float distance = length(gl_Position);
    float fogFactor = 1.0F - exp(-fogs[brightness].fogDensity * distance);
    fragColor.xyz *= (0.6f + 0.4f * brightness) * alpha * bright;
    fragColor.w *= alpha;
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
    uint blockType = BlockType16_Brightness6a[0] >> 16;
    uint brightnessBits = (BlockType16_Brightness6a[0] >> 10) & 0x3F;
    vec4 minusPos = vec4(hitPos - vec3(0.0001f), 0.0f) + blocks[blockType].bounds[0];
    vec4 plusPos = vec4(hitPos + vec3(0.0001f), 0.0f) + blocks[blockType].bounds[1];
    vec4 camPos = inverse(view)[3];
    uint brightness = 0;
    uint c = 0;
    vec4 basicColor = vec4(0.0F, 0.0F, 0.0F, 0.0F);
    if (minusPos.y > camPos.y)
    {
        brightness = (brightnessBits >> 1) & 0x1;
        fragTextureIndex = blocks[blockType].indexTextures_Colors[1].x;
        c = blocks[blockType].indexTextures_Colors[1].y;
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
        fragTextureIndex = blocks[blockType].indexTextures_Colors[0].x;
        c = blocks[blockType].indexTextures_Colors[0].y;
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
        fragTextureIndex = blocks[blockType].indexTextures_Colors[3].x;
        c = blocks[blockType].indexTextures_Colors[3].y;
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
        fragTextureIndex = blocks[blockType].indexTextures_Colors[2].x;
        c = blocks[blockType].indexTextures_Colors[2].y;
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
        fragTextureIndex = blocks[blockType].indexTextures_Colors[5].x;
        c = blocks[blockType].indexTextures_Colors[5].y;
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
        fragTextureIndex = blocks[blockType].indexTextures_Colors[4].x;
        c = blocks[blockType].indexTextures_Colors[4].y;
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