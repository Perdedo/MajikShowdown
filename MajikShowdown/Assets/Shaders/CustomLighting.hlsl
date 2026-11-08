#ifndef CUSTOM_LIGTHING_INCLUDE
#define CUSTOM_LIGTHING_INCLUDE

struct CustomData 
{
    float3 normalWs;
    float3 albedo;
    float4 shadowCord;
    float3 positionWS;
};

#if !defined(SHADERGRAPH_PREVIEW)
float3 CalculateDiffuse(CustomData d, Light light)
{
    float3 radiance = light.color * light.shadowAttenuation;
    float diffuse = saturate(dot(d.normalWs, light.direction));
    float3 color = d.albedo * radiance * diffuse;;

    return color;
}
#endif

float3 CalculateCustomLigthing(CustomData d)
{
    #if defined(SHADERGRAPH_PREVIEW)
        float3 lightdir = float3(0.5,0.5,0);
        float3 intensity = saturate(dot(d.normalWs, lightdir));
        return d.albedo * intensity;
    #else
        Light mainLight = GetMainLight(d.shadowCord, d.positionWS, 1);

        float3 color = 0;
        color += CalculateDiffuse(d, mainLight);
        return color;
    #endif
}

void CalculateCustomLigthing_float(float3 Position, float3 Normal, float3 Albedo, out float3 Color){
    CustomData d;
    d.albedo = Albedo;
    d.normalWs = Normal;
    d.positionWS = Position;

    #if defined(SHADERGRAPH_PREVIEW)
        d.shadowCord = 0;
    #else
        float4 positionCS = TransformWorldToHClip(Position);
        #if defined(SHADOWS_SCREEN)
            D.shadowCord = ComputeScreenPos(positionCS);
        #else
            d.shadowCord = TransformWorldToShadowCoord(positionCS);
        #endif
    #endif

    Color = CalculateCustomLigthing(d);
}

#endif