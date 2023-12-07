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
    gl_Position = view * gl_Position;
    float distance = length(gl_Position);
    float fogFactor = 1.0F - exp(-fogDensity[0] * distance);
    fragColor = mix(color[0], fogColor[0], fogFactor);
    gl_Position = projection * gl_Position;
    EmitVertex();
}

void main()
{
    vec3 plusOffset = boundsMax[0] + vec3(0.0001f);
    vec3 minusOffset = boundsMin[0] - vec3(0.0001f);
    vec4 camPos = inverse(view)[3];
    if (hitPos.x > camPos.x)
    {
        gl_Position = vec4(minusOffset.x + hitPos.x, minusOffset.y + hitPos.y, minusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(minusOffset.x + hitPos.x, hitPos.y + minusOffset.y, plusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(minusOffset.x + hitPos.x, plusOffset.y + hitPos.y, hitPos.z + minusOffset.z, 1.0);
        ApplyFog();
        gl_Position = vec4(minusOffset.x + hitPos.x, hitPos.y + plusOffset.y, hitPos.z + plusOffset.z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    else
    {
        gl_Position = vec4(hitPos.x + plusOffset.x, minusOffset.y + hitPos.y, minusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + plusOffset.x, plusOffset.y + hitPos.y, hitPos.z + minusOffset.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + plusOffset.x, hitPos.y + minusOffset.y, plusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + plusOffset.x, hitPos.y + plusOffset.y, hitPos.z + plusOffset.z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    if (hitPos.y > camPos.y)
    {
        gl_Position = vec4(minusOffset.x + hitPos.x, minusOffset.y + hitPos.y, minusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(plusOffset.x + hitPos.x, minusOffset.y + hitPos.y, hitPos.z + minusOffset.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + minusOffset.x, minusOffset.y + hitPos.y, plusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + plusOffset.x, minusOffset.y + hitPos.y, hitPos.z + plusOffset.z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    else
    {
        gl_Position = vec4(minusOffset.x + hitPos.x, hitPos.y + plusOffset.y, minusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + minusOffset.x, hitPos.y + plusOffset.y, plusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(plusOffset.x + hitPos.x, hitPos.y + plusOffset.y, hitPos.z + minusOffset.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + plusOffset.x, hitPos.y + plusOffset.y, hitPos.z + plusOffset.z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    if (hitPos.z > camPos.z)
    {
        gl_Position = vec4(minusOffset.x + hitPos.x, minusOffset.y + hitPos.y, minusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + minusOffset.x, plusOffset.y + hitPos.y, minusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(plusOffset.x + hitPos.x, hitPos.y + minusOffset.y, minusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + plusOffset.x, hitPos.y + plusOffset.y, minusOffset.z + hitPos.z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
    else
    {
        gl_Position = vec4(minusOffset.x + hitPos.x, minusOffset.y + hitPos.y, hitPos.z + plusOffset.z, 1.0);
        ApplyFog();
        gl_Position = vec4(plusOffset.x + hitPos.x, hitPos.y + minusOffset.y, hitPos.z + plusOffset.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + minusOffset.x, plusOffset.y + hitPos.y, hitPos.z + plusOffset.z, 1.0);
        ApplyFog();
        gl_Position = vec4(hitPos.x + plusOffset.x, hitPos.y + plusOffset.y, hitPos.z + plusOffset.z, 1.0);
        ApplyFog();
        EndPrimitive();
    }
}