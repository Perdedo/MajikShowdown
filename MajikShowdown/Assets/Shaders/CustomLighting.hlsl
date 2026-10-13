
#ifndef CUSTOM_LIGTHING_INCLUDE
#define CUSTOM_LIGTHING_INCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

struct CustomData
{
    float3 worldPosition;
    float4 shadowCord;
    float3 normalWs;
    float3 albedo;
};

#if !defined(SHADERGRAPH_PREVIEW)
    float3 LightHandle(CustomData d, Light light)
    {
        
        float3 radiance = light.color * light.shadowAttenuation;
        float3 diffuse = saturate(dot(d.normalWs,light.direction));
        float3 color = d.albedo * radiance * diffuse;

        return color;

    }
#endif

float3 CalculeteLigthing(CustomData d)
{
    #if defined(SHADERGRAPH_PREVIEW)
        float3 lightdir = float3(0.5,0.5,0);
        float3 intensity = saturate(dot(d.normalWs, lightdir));
        return d.albedo * intensity;
    #else
        Light mainLight = GetMainLight(d.shadowCord, d.worldPosition, 1);

        float3 color = 0;
        color += LightHandle(d, mainLight);
        #if defined(ADDITIONAL_LIGHTS)
            uint numLights = GetAdditionalLightsCount();
            for(uint i = 0; i < numLights; i++)
            {
                Light light = GetAdditionalLight(i, d.worldPosition, 1);
                color += LightHandle(d,mainLight);
            }
        #endif
        return color;
    #endif
}

void CalculeteLigthing_float(float3 position, float3 normal, float3 albedo, out float3 color)
{
    CustomData d;
    d.albedo = albedo;
    d.normalWs = normal;
    d.worldPosition = position;

    #if defined(SHADERGRAPH_PREVIEW)
        d.shadowCord = 0;
    #else
        float4 clipPos = TransformWorldToHClip(position);
        #if defined(SHADOWS_SCREEN)
            d.shadowCord = ComputeScreenPos(clipPos);
        #else
            d.shadowCord = TransformWorldToShadowCoord(position);
        #endif
    #endif
    
    color = CalculeteLigthing(d);
}
#endif