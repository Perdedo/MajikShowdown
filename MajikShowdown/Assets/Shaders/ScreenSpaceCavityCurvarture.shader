Shader "Custom/ScreenSpaceCavityCurvarture"
{      Properties
    {
        [Radius][IntRange] _Radius("Radius", Range(0,10)) = 1
        [Power][IntRange] _Power("Distance power", Range(1,10)) = 5
        [Sensitivity] _Sensitivity("Angle sensitivity", Range(1,5)) = 2.5
        [Multiplier] _Multiplier("Edge intensity multiplier", Range(0,10)) = 0.6
        [Sharpness] _Sharpness("Sharpness", Range(0,1)) = 0.9
        [Opacity] _Opacity("Opacity", Range(0,1)) = 1
    }
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "ScreenSpaceCavityCurvarture"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment Frag

            CBUFFER_START(UnityPerMaterial)
                int _Radius;
                int _Power;
                float _Sensitivity;
                float _Sharpness;
                float _Multiplier;
                float _Opacity;
                float2 _Vec;
            CBUFFER_END


            //That's the blend/Soft light node code from unity
            void BlendSoftLight(float4 Base, float4 Blend, float Opacity, out float4 Out)
            {
                float4 result1 = 2.0 * Base * Blend + Base * Base * (1.0 - 2.0 * Blend);
                float4 result2 = sqrt(Base) * (2.0 * Blend - 1.0) + 2.0 * Base * (1.0 - Blend);
                float4 zeroOrOne = step(0.5, Blend);
                Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
                Out = lerp(Base, Out, Opacity);
            }

            float4 overlay(float3 base, float3 blend)
            {
                return float4(lerp(2.0 * base * blend, 1.0 - 2.0 * (1.0 - blend) * (1.0 - blend), clamp(base, 0.0, 1.0)), 1);
            }


            //Sample the world normal and coverts to view space normal
            float2 SampleSceneNormalBuffer(float2 uv, float3x3 viewMatrix)
            {
                float3 normal = SampleSceneNormals(uv);
                float3 vNormal =  (mul(viewMatrix, normal));
                return vNormal.xy;
            }

            //Calculates the curvature using prewitt operator
            //The sensitivity and multiplier controls the tolerance in change of the surface angle 
            float CalculateCurvature(float2 left, float2 right, float2 down, float2 up, float sensitivity, float multiplier)
            {
                float resultX = left.x - right.x;
                float resultY = up.y - down.y;
                float totalResult = resultX + resultY;

                float curvature = 0.5 + sign(totalResult) * pow(abs(totalResult * multiplier), sensitivity);
                return clamp(curvature, 0.0, 1.0);
            }

            
            float GetCurvatureAtPoint(float2 uv, float sensitivity, float multiplier, float3x3 viewMatrix, float dis)
            {
                if(dis < 0.75)
                {
                    dis = 0.75;
                }
                float2 leftRight = float2((1.0 * dis) / _ScreenParams.x, 0);
                float2 upDown = float2(0, (1.0  * dis ) / _ScreenParams.y);

                float2 left = SampleSceneNormalBuffer(uv + leftRight, viewMatrix);
                float2 right = SampleSceneNormalBuffer(uv - leftRight, viewMatrix);
                float2 down = SampleSceneNormalBuffer(uv - upDown, viewMatrix);
                float2 up = SampleSceneNormalBuffer(uv + upDown, viewMatrix);

                return CalculateCurvature(left, right, down, up, sensitivity, multiplier);

            }

            //Calculates the average curvature from around any given point
            void GetAverageCurvature(float2 screenPosition, int radius, float sensitivity, float multiplier, float sharpness, out float curvature, float dis)
            {
                float3x3 viewMatrix = (float3x3)UNITY_MATRIX_V;
                float totalWeight = 0.0;
                curvature = 0.0;
                sharpness = clamp(1.0 - sharpness, 0.0001, 1.0);
                for (int i = -radius; i <= radius; i++)
                {
                    for (int j = -radius; j <= radius; j++)
                    {
                        float2 pixelOffset = float2(i, j);
                        float2 uvOffset = pixelOffset / _ScreenParams.xy;
                        float weight = 1 / (dot(pixelOffset, pixelOffset) + sharpness);
                        totalWeight += weight;
                        curvature += weight * GetCurvatureAtPoint(screenPosition + uvOffset, sensitivity, multiplier, viewMatrix, dis);
                    }
                }

                curvature /= totalWeight;
            }
            float EaseFunc(float x)
            {
                return pow(1 - x, _Power);
            }
            float4 Frag (Varyings input) : SV_Target
            {
                float3x3 viewMatrix = (float3x3)UNITY_MATRIX_V;
                float2 uv = input.texcoord;
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);
                float4 finalColor;
                float curvature;
                float depth = SampleSceneDepth(uv);
                float linearDepth  = Linear01Depth(depth, _ZBufferParams);
                float opacM = 1 - smoothstep(0.05, 0.5, linearDepth);

                GetAverageCurvature(uv, _Radius, _Sensitivity, _Multiplier, _Sharpness, curvature, EaseFunc(linearDepth));

                
                //BlendSoftLight(color, curvature, _Opacity * opacM, finalColor);
                BlendSoftLight(color, curvature, _Opacity, finalColor);
                return finalColor;
            }
            
            ENDHLSL
        }
    }
}
