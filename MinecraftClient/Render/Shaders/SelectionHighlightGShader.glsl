#version 460 core
layout(points) in;
layout(triangle_strip, max_vertices = 12) out;
layout(binding = 5) uniform CameraInfo { mat4 view; mat4 projection; };

uniform vec3 hitPos;

in vec3 boundsMin[];
in vec3 boundsMax[];
in float fogDensity[];
in vec4 color[];
in vec4 fogColor[];

out vec4 fragColor;

void ApplyFog()
{
    float distance = length(gl_Position);
    float fogFactor = 1.0F - exp(-fogDensity[0] * distance);
    fragColor = mix(color[0], fogColor[0], fogFactor);
    gl_Position = projection * gl_Position;
    EmitVertex();
}

void main()
{
    vec3 camPos = inverse(view)[3].xyz;
    if(hitPos.x > camPos.x)
    {
        gl_Position = vec4(boundsMin[0].x + hitPos.x, boundsMin[0].y + hitPos.y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(boundsMin[0].x + hitPos.x, hitPos.y + boundsMax[0].y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(boundsMin[0].x + hitPos.x, boundsMin[0].y + hitPos.y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        gl_Position = vec4(boundsMin[0].x + hitPos.x, hitPos.y + boundsMax[0].y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    else
    {
        gl_Position = vec4(hitPos.x + boundsMax[0].x, boundsMin[0].y + hitPos.y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, boundsMin[0].y + hitPos.y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, hitPos.y + boundsMax[0].y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, hitPos.y + boundsMax[0].y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    if(hitPos.y > camPos.y)
    {
        gl_Position = vec4(boundsMin[0].x + hitPos.x, boundsMin[0].y + hitPos.y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(boundsMin[0].x + hitPos.x, boundsMin[0].y + hitPos.y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, boundsMin[0].y + hitPos.y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, boundsMin[0].y + hitPos.y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    else
    {
        gl_Position = vec4(boundsMin[0].x + hitPos.x, hitPos.y + boundsMax[0].y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, hitPos.y + boundsMax[0].y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(boundsMin[0].x + hitPos.x, hitPos.y + boundsMax[0].y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, hitPos.y + boundsMax[0].y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    if(hitPos.z > camPos.z)
    {
        gl_Position = vec4(boundsMin[0].x + hitPos.x, boundsMin[0].y + hitPos.y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, boundsMin[0].y + hitPos.y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(boundsMin[0].x + hitPos.x, hitPos.y + boundsMax[0].y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, hitPos.y + boundsMax[0].y, boundsMin[0].z + hitPos.z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    else
    {
        gl_Position = vec4(boundsMin[0].x + hitPos.x, boundsMin[0].y + hitPos.y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        gl_Position = vec4(boundsMin[0].x + hitPos.x, hitPos.y + boundsMax[0].y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, boundsMin[0].y + hitPos.y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + boundsMax[0].x, hitPos.y + boundsMax[0].y, hitPos.z + boundsMax[0].z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
}