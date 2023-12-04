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

in uint blockType[];
in uint brightnessBits[];
in mat4 transformationMatrix[];

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
    vec3 boundsMin = blocks[blockType[0]].bounds[0].xyz;
    vec3 boundsMax = blocks[blockType[0]].bounds[1].xyz;
    vec4 pos = gl_in[0].gl_Position;
    vec4 finalPos = transformationMatrix[0] * pos;
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
    if (finalPos.y > camPos.y)
    {
        uint brightness = (brightnessBits[0] >> 1) & 0x1;
        fragTextureIndex = blocks[blockType[0]].indexTextures_Colors[1].x;
        uint c = blocks[blockType[0]].indexTextures_Colors[1].y;
        vec4 basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else
    {
        uint brightness = brightnessBits[0] & 0x1;
        fragTextureIndex = blocks[blockType[0]].indexTextures_Colors[0].x;
        uint c = blocks[blockType[0]].indexTextures_Colors[0].y;
        vec4 basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    if (finalPos.x > camPos.x)
    {
        uint brightness = (brightnessBits[0] >> 3) & 0x1;
        fragTextureIndex = blocks[blockType[0]].indexTextures_Colors[3].x;
        uint c = blocks[blockType[0]].indexTextures_Colors[3].y;
        vec4 basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else
    {
        uint brightness = (brightnessBits[0] >> 2) & 0x1;
        fragTextureIndex = blocks[blockType[0]].indexTextures_Colors[2].x;
        uint c = blocks[blockType[0]].indexTextures_Colors[2].y;
        vec4 basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    if (finalPos.z > camPos.z)
    {
        uint brightness = brightnessBits[0] >> 5;
        fragTextureIndex = blocks[blockType[0]].indexTextures_Colors[5].x;
        uint c = blocks[blockType[0]].indexTextures_Colors[5].y;
        vec4 basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMin.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMin.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
    else
    {
        uint brightness = (brightnessBits[0] >> 4) & 0x1;
        fragTextureIndex = blocks[blockType[0]].indexTextures_Colors[4].x;
        uint c = blocks[blockType[0]].indexTextures_Colors[4].y;
        vec4 basicColor = vec4((c >> 24) / 255.0F, (c >> 16 & 0xFF) / 255.0F, (c >> 8 & 0xFF) / 255.0F, (c & 0xFF) / 255.0F);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(0u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMin.y, boundsMax.z, 0.0F));
        NextVertex(1u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(2u, brightness);
        fragColor = basicColor;
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax.x, boundsMax.y, boundsMax.z, 0.0F));
        NextVertex(3u, brightness);
        EndPrimitive();
    }
}