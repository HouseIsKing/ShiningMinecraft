#version 460 core
#extension GL_ARB_bindless_texture : enable

layout(bindless_sampler) uniform;
layout(binding = 3) uniform textureSamplers { sampler2D textures[16]; };

in vec2 fragTexture;
in vec4 fragColor;
in flat uint fragTextureIndex;

out vec4 FragColor;

void main()
{
    vec4 textureColor = texture(textures[fragTextureIndex] , fragTexture);
	FragColor = fragColor * textureColor;
}
