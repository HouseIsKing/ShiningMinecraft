#version 460 core
#extension GL_ARB_bindless_texture : enable

layout(bindless_sampler) uniform;
layout(binding = 3) uniform textureSamplers { sampler2D textures[16]; };

in vec2 outTexture;
in vec4 color;
in flat uint textureIndex;

out vec4 FragColor;

void main()
{
    vec4 textureColor = texture(textures[textureIndex] , outTexture);
	FragColor = color * textureColor;
	//FragColor = color;
}
