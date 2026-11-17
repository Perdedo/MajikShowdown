#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#if !defined(SHADERGRAPH_PREVIEW)
	#if SHADERPASS != SHADERPASS_FORWARD && SHADERPASS != SHADERPASS_GBUFFER

    	#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
    	#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
		#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
		#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
		#pragma multi_compile _ _CLUSTER_LIGHT_LOOP

	#endif
#endif

void ShadowMask_half(float2 lightmapUV, out half4 Shadowmask)
{
	#if defined(SHADERGRAPH_PREVIEW)
		Shadowmask = half4(1,1,1,1);
	#else
		OUTPUT_LIGHTMAP_UV(lightmapUV, unity_LightmapST, lightmapUV);
		Shadowmask = SAMPLE_SHADOWMASK(lightmapUV);
	#endif
}

void MainLightShadows_float(float3 WorldPos, half4 Shadowmask, out float ShadowAtten){
	#ifdef SHADERGRAPH_PREVIEW
		ShadowAtten = 1;
	#else
		#if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
			float4 shadowCoord = ComputeScreenPos(TransformWorldToHClip(WorldPos));
		#else
			float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
		#endif
			ShadowAtten = MainLightShadow(shadowCoord, WorldPos, Shadowmask, _MainLightOcclusionProbes);
	#endif
}
	
#ifndef SHADERGRAPH_PREVIEW
	float Attenuation (int lightIndex, float3 positionWS, float pointBands, float spotBands)
	{
		#if !USE_CLUSTER_LIGHT_LOOP
			lightIndex = GetPerObjectLightIndex(lightIndex);
		#endif
		#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
			float4 lightPositionWS = _AdditionalLightsBuffer[lightIndex].position;
			half4 spotDirection = _AdditionalLightsBuffer[lightIndex].spotDirection;
			half4 distanceAndSpotAttenuation = _AdditionalLightsBuffer[lightIndex].attenuation;
		#else
			float4 lightPositionWS = _AdditionalLightsPosition[lightIndex];
			half4 spotDirection = _AdditionalLightsSpotDir[lightIndex];
			half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[lightIndex];
		#endif

		//Point
		float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
		float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);
		float range = rsqrt(distanceAndSpotAttenuation.x);
		float dist = sqrt(distanceSqr) / range;

		// Spot
		half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
		half SdotL = dot(spotDirection.xyz, lightDirection);
		half spotAtten = saturate(SdotL * distanceAndSpotAttenuation.z + distanceAndSpotAttenuation.w);
		spotAtten *= spotAtten;
		float maskSpotToRange = step(dist, 1);

		// Atten
		bool isSpot = (distanceAndSpotAttenuation.z > 0);
		return isSpot ? 
			//step(0.01, spotAtten) :		// cheaper if you just want "1" band for spot lights
			(floor(spotAtten * spotBands) / spotBands) * maskSpotToRange :
			saturate(1.0 - floor(dist * pointBands) / pointBands);

	}
#endif
void AdditionalLights_float(float3 WorldPosition, float3 WorldNormal, float3 WorldView, half4 Shadowmask, float PointLightBands, float SpotLightBands, out float3 Diffuse)
{
	float3 diffuseColor = 0;
	#ifndef SHADERGRAPH_PREVIEW
		uint pixelLightCount = GetAdditionalLightsCount();
		uint meshRenderingLayers = GetMeshRenderingLayer();

		#if USE_CLUSTER_LIGHT_LOOP
		for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++) 
		{
			CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
			Light light = GetAdditionalLight(lightIndex, WorldPosition, Shadowmask);
			#ifdef _LIGHT_LAYERS
				if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
			#endif
			{
				if (PointLightBands <= 1 && SpotLightBands <= 1){
					diffuseColor += light.color * step(0.0001, light.distanceAttenuation * light.shadowAttenuation);
				}else{
					diffuseColor += light.color * light.shadowAttenuation * Attenuation(lightIndex, WorldPosition, PointLightBands, SpotLightBands);
				}
			}
		}
		#endif

		InputData inputData = (InputData)0;
		float4 screenPos = ComputeScreenPos(TransformWorldToHClip(WorldPosition));
		inputData.normalizedScreenSpaceUV = screenPos.xy / screenPos.w;
		inputData.positionWS = WorldPosition;
		LIGHT_LOOP_BEGIN(pixelLightCount)
			Light light = GetAdditionalLight(lightIndex, WorldPosition, Shadowmask);
				#ifdef _LIGHT_LAYERS
					if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
				#endif
			{
				if (PointLightBands <= 1 && SpotLightBands <= 1){
					// Solid colour lights
					diffuseColor += light.color * step(0.0001, light.distanceAttenuation * light.shadowAttenuation);
				}else{
					// Multiple bands
					diffuseColor += light.color * light.shadowAttenuation * Attenuation(lightIndex, WorldPosition, PointLightBands, SpotLightBands);
				}
			}
		LIGHT_LOOP_END
	#endif
	Diffuse = diffuseColor;
}
#endif
