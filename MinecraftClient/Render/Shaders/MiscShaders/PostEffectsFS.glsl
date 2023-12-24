#version 460 core
out vec4 FragColor;
  
in vec2 TexCoords;

uniform sampler2D hdrBuffer;
uniform sampler2D depthBuffer;

void main()
{
    vec4 hdrColor = texture(hdrBuffer, TexCoords);
    if (texture(depthBuffer, TexCoords).x < 0.9999)
    {
        vec4 sum = vec4(0);
        int j;
        int i;
        for( i= -3 ;i < 3; i++)
        // reduce loop count for performance
        
        {
            for (j = -3; j < 3; j++)
            {
                vec2 helper = TexCoords + vec2(j, i)*0.002;
                if(helper.x < 0.0 || helper.y < 0.0 || helper.x > 1.0 || helper.y > 1.0)
                {
                    continue;
                }
                sum += texture(hdrBuffer, helper) * 0.20;         
                // 0.20 = less , 0.25 = more
                // change this value for the effect strength
            }
        }
        if (hdrColor.r < 0.3)
        {
           hdrColor = sum*sum*0.012 + hdrColor;
        }
        else
        {
            if (hdrColor.r < 0.5)
            {
                hdrColor = sum*sum*0.009 + hdrColor;
            }
            else
            {
                hdrColor = sum*sum*0.0075 + hdrColor;        
            }
        }
    }

    const float gamma = 0.75;
    vec3 result = vec3(1.0) - exp(-hdrColor.rgb * 2.0);
    result = pow(result, vec3(1.0 / gamma));
    FragColor = vec4(result, hdrColor.a);
}