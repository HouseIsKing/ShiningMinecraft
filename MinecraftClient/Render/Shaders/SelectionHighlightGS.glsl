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
uniform vec3 hitPos;

in uint BlockType16_Brightness6a[];

out vec4 fragColor;

void ApplyFog(uint brightness)
{
    vec4 fogColor = fogs[brightness].fogColor;
    float fogDensity = fogs[brightness].fogDensity;
    float c = (0.6F + 0.4F * (brightness % 2)) * alpha;
    vec4 color = vec4(c, c, c, alpha);
    gl_Position = view * gl_Position;
    float distance = length(gl_Position);
    float fogFactor = 1.0F - exp(-fogDensity * distance);
    fragColor = mix(color, fogColor, fogFactor);
    gl_Position = projection * gl_Position;
    EmitVertex();
}

void main()
{
    uint blockType = BlockType16_Brightness6a[0] >> 16;
    uint brightnessBits = (BlockType16_Brightness6a[0] >> 10) & 0x3F;
    vec4 minusPos = vec4(hitPos - vec3(0.0001f), 0.0f) + blocks[blockType].bounds[0];
    vec4 plusPos = vec4(hitPos + vec3(0.0001f), 0.0f) + blocks[blockType].bounds[1];
    vec4 camPos = inverse(view)[3];
    uint brightness;
    if (minusPos.x > camPos.x)
    {
        brightness = (brightnessBits >> 3) & 0x1;
        gl_Position = vec4(minusPos.x, minusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(minusPos.x, minusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(minusPos.x, plusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(minusPos.x, plusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        EndPrimitive();
    }
    else if (plusPos.x < camPos.x)
    {
        brightness = (brightnessBits >> 2) & 0x1;
        gl_Position = vec4(plusPos.x, minusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, plusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, minusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, plusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        EndPrimitive();
    }
    if (minusPos.y > camPos.y)
    {
        brightness = (brightnessBits >> 1) & 0x1;
        gl_Position = vec4(minusPos.x, minusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, minusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(minusPos.x, minusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, minusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        EndPrimitive();
    }
    else if (plusPos.y < camPos.y)
    {
        brightness = brightnessBits & 0x1;
        gl_Position = vec4(minusPos.x, plusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(minusPos.x, plusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, plusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, plusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        EndPrimitive();
    }
    if (minusPos.z > camPos.z)
    {
        brightness = (brightnessBits >> 5) & 0x1;
        gl_Position = vec4(minusPos.x, minusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(minusPos.x, plusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, minusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, plusPos.y, minusPos.z, 1.0);
        ApplyFog(brightness);
        EndPrimitive();
    }
    else if (plusPos.z < camPos.z)
    {
        brightness = (brightnessBits >> 4) & 0x1;
        gl_Position = vec4(minusPos.x, minusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, minusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(minusPos.x, plusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        gl_Position = vec4(plusPos.x, plusPos.y, plusPos.z, 1.0);
        ApplyFog(brightness);
        EndPrimitive();
    }
}