#version 460 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aUV;
layout (location = 2) in vec4 aColor;
layout (location = 3) in uint aIndex;
layout (location = 4) in uint brightness;
layout (location = 5) in uint SpecialEffects;

struct Fog
{
    vec4 fogColor;
    float fogDensity;
};

layout(binding = 1) uniform fogBlock { Fog fogs[2]; };
uniform mat4 transformationMatrix;
uniform mat4 view;
uniform mat4 projection;
uniform uint worldTime;
out vec2 outTexture;
out vec4 color;
out uint samplerHandle;

void main()
{
    vec3 pos = aPos;
    if((SpecialEffects & 0x1) == 1)
    {
        float speed = 2.0F;
        float pi = 3.14159265359F;
        float magnitude = (sin((aPos.y + aPos.x + worldTime * pi / ((28.0F) * speed))) * 0.15F + 0.15F) * 0.20F;
        float d0 = sin(worldTime * pi / (112.0F * speed)) * 3.0F - 1.5F;
        float d1 = sin(worldTime * pi / (142.0F * speed)) * 3.0F - 1.5F;
        float d2 = sin(worldTime * pi / (132.0F * speed)) * 3.0F - 1.5F;
        float d3 = sin(worldTime * pi / (122.0F * speed)) * 3.0F - 1.5F;
        pos.x += sin((worldTime * pi / (18.0F * speed)) + (-aPos.x + d0) * 1.6F + (aPos.z + d1) * 1.6F) * magnitude;
        pos.z += sin((worldTime * pi / (17.0F * speed)) + (aPos.z + d2) * 1.6F + (-aPos.x + d3) * 1.6F) * magnitude;
        pos.y += sin((worldTime * pi / (11.0F * speed)) + (aPos.z + d2) + (aPos.x + d3)) * (magnitude / 2.0F);
    }
    vec4 posRelativeToCamera = view * transformationMatrix * vec4(pos, 1.0);
	gl_Position = projection * posRelativeToCamera;
	outTexture = aUV;
	samplerHandle = aIndex;
	float distance = length(posRelativeToCamera.xyz);
	vec4 fogColor = fogs[brightness].fogColor;
    float fogFactor = 1.0F - exp(-fogs[brightness].fogDensity * distance);
    color = mix(aColor, fogColor, fogFactor);
}