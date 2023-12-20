#version 460 core
out vec4 FragColor;
  
in vec2 TexCoords;

uniform sampler2D hdrBuffer;

void main()
{
    const float gamma = 0.6;             
    vec3 hdrColor = texture(hdrBuffer, TexCoords).rgb;
    vec3 result = vec3(1.0) - exp(-hdrColor * 3.0);
    result = pow(result, vec3(1.0 / gamma));
    FragColor = vec4(result, 1.0);
}