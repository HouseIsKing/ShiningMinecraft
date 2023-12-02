#version 460 core
layout (points) in;
layout (triangle_strip, max_vertices = 4) out;
layout(binding = 5) uniform CameraInfo { mat4 view; mat4 projection; };
layout(binding = 6) uniform WorldInfo { uint chunkWidth; uint chunkHeight; uint chunkDepth; uint worldTime; };

in vec3 boundsMin[];
in vec3 boundsMax[];
in flat uint textureIndex[];
in uint Face[];
in float fogDensity[];
in vec4 color[];
in vec4 fogColor[];
in mat4 transformationMatrix[];

out vec4 fragColor;
out vec2 fragTexture;
out flat uint fragTextureIndex;

void ApplyFog()
{
    float distance = length(gl_Position);
    float fogFactor = 1.0F - exp(-fogDensity[0] * distance);
    fragColor = mix(color[0], fogColor[0], fogFactor);
    gl_Position = projection * gl_Position;
}

void NextVertex(uint index)
{
    ApplyFog();
    fragTexture = vec2(index % 2, index / 2);
    EmitVertex();
}

void main()
{
    fragTextureIndex = textureIndex[0];
    vec4 pos = gl_in[0].gl_Position;
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
    if (Face[0] == 0u)
    {
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMax[0].y, boundsMin[0].z, 0.0F));
        NextVertex(0u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMax[0].y, boundsMax[0].z, 0.0F));
        NextVertex(1u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMax[0].y, boundsMin[0].z, 0.0F));
        NextVertex(2u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMax[0].y, boundsMax[0].z, 0.0F));
        NextVertex(3u);
        EndPrimitive();
    }
    if (Face[0] == 1u)
    {
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMin[0].y, boundsMin[0].z, 0.0F));
        NextVertex(0u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMin[0].y, boundsMin[0].z, 0.0F));
        NextVertex(2u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMin[0].y, boundsMax[0].z, 0.0F));
        NextVertex(1u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMin[0].y, boundsMax[0].z, 0.0F));
        NextVertex(3u);
        EndPrimitive();
    }
    if (Face[0] == 2u)
    {
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMin[0].y, boundsMin[0].z, 0.0F));
        NextVertex(0u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMax[0].y, boundsMin[0].z, 0.0F));
        NextVertex(2u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMin[0].y, boundsMax[0].z, 0.0F));
        NextVertex(1u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMax[0].y, boundsMax[0].z, 0.0F));
        NextVertex(3u);
        EndPrimitive();
    }
    if (Face[0] == 3u)
    {
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMin[0].y, boundsMin[0].z, 0.0F));
        NextVertex(0u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMin[0].y, boundsMax[0].z, 0.0F));
        NextVertex(1u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMax[0].y, boundsMin[0].z, 0.0F));
        NextVertex(2u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMax[0].y, boundsMax[0].z, 0.0F));
        NextVertex(3u);
        EndPrimitive();
    }
    if (Face[0] == 4u)
    {
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMin[0].y, boundsMax[0].z, 0.0F));
        NextVertex(0u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMin[0].y, boundsMax[0].z, 0.0F));
        NextVertex(1u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMax[0].y, boundsMax[0].z, 0.0F));
        NextVertex(2u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMax[0].y, boundsMax[0].z, 0.0F));
        NextVertex(3u);
        EndPrimitive();
    }
    if (Face[0] == 5u)
    {
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMin[0].y, boundsMin[0].z, 0.0F));
        NextVertex(0u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMin[0].x, boundsMax[0].y, boundsMin[0].z, 0.0F));
        NextVertex(2u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMin[0].y, boundsMin[0].z, 0.0F));
        NextVertex(1u);
        gl_Position = view * transformationMatrix[0] * (pos + vec4(boundsMax[0].x, boundsMax[0].y, boundsMin[0].z, 0.0F));
        NextVertex(3u);
        EndPrimitive();
    }
}