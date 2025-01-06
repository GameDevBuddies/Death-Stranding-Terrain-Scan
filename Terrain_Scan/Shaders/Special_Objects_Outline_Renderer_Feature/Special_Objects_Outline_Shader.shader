Shader "GameDevBuddies/Special_Objects_Outline_Shader"
{    
    Properties 
    {
        [HDR] _OutlineColor ("Outline Color", Color) = (0,0,0,0)
        _OutlineThickness ("Outline Thickness", Range(0.0, 20.0)) = 2.0
        _OutlineVisibility ("Outline Visibility", Range(0.0, 1.0)) = 1.0
        _SobelEffectMultiplier ("Sobel Multiplier", Range(1.0, 10.0)) = 2.0
        _SobelEffectBias ("Sobel Bias", Range(1.0, 10.0)) = 4.0
    }

    SubShader
    {
        Tags 
        {
            "RenderType" = "Opaque"
        }

        Pass 
        {
            Name "Special_Objects_Outline"

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment OutlineFragmentFunction

            TEXTURE2D_X_FLOAT(_SpecialObjectsNormals);
            SAMPLER(sampler_SpecialObjectsNormals);
            
            TEXTURE2D_X_FLOAT(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);

            float4 _OutlineColor;
            float _OutlineThickness;
            float _OutlineVisibility;
            float _SobelEffectMultiplier;
            float _SobelEffectBias;

            float4 SampleNormalsTexture(float2 uv) 
            {
                return SAMPLE_TEXTURE2D_X(_SpecialObjectsNormals, sampler_SpecialObjectsNormals, UnityStereoTransformScreenSpaceTex(uv));
            }

            float3 SobelSampleNormals(float2 uv, float2 pixelOffset)
            {
                float2 horizontalOffset = float2(pixelOffset.x, 0);
                float2 verticalOffset = float2(0, pixelOffset.y);

                float3 center = SampleNormalsTexture(uv).rgb;
                float3 left   = SampleNormalsTexture(uv - horizontalOffset).rgb;
                float3 right  = SampleNormalsTexture(uv + horizontalOffset).rgb;
                float3 up     = SampleNormalsTexture(uv + verticalOffset).rgb;
                float3 down   = SampleNormalsTexture(uv - verticalOffset).rgb;

                return (center - left) + (center - right) + (center - up) + (center - down);
            }

            float SobelSampleDepth(float2 uv, float2 pixelOffset)
            {
                float2 horizontalOffset = float2(pixelOffset.x, 0);
                float2 verticalOffset = float2(0, pixelOffset.y);

                float center = SampleNormalsTexture(uv).a;
                float left   = SampleNormalsTexture(uv - horizontalOffset).a;
                float right  = SampleNormalsTexture(uv + horizontalOffset).a;
                float up     = SampleNormalsTexture(uv + verticalOffset).a;
                float down   = SampleNormalsTexture(uv - verticalOffset).a;

                return (center - left) + (center - right) + (center - up) + (center - down);
            }

            float4 SobelSampleNormalsAndDepth(float2 uv, float2 pixelOffset)
            {
                float2 horizontalOffset = float2(pixelOffset.x, 0);
                float2 verticalOffset = float2(0, pixelOffset.y);

                float4 center = SampleNormalsTexture(uv);
                float4 left   = SampleNormalsTexture(uv - horizontalOffset);
                float4 right  = SampleNormalsTexture(uv + horizontalOffset);
                float4 up     = SampleNormalsTexture(uv + verticalOffset);
                float4 down   = SampleNormalsTexture(uv - verticalOffset);

                return (center - left) + (center - right) + (center - up) + (center - down);
            }

            float4 OutlineFragmentFunction(Varyings input) : SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);

                // Raw camera frame color.
                float3 cameraColor = SAMPLE_TEXTURE2D_X(_CameraColorTexture, sampler_CameraColorTexture, UnityStereoTransformScreenSpaceTex(input.texcoord)).xyz;
                
                // Sobel based on object normals.
                float2 pixelOffset = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y) * _OutlineThickness;
                float3 sobelNormalVec = SobelSampleNormals(input.texcoord, pixelOffset);

                // DEBUG: Visualization of the normals sobel effect.
                //return float4(sobelNormalVec.xyz, 1.0);

                // Sobel based on object depth.
                float depth = SAMPLE_TEXTURE2D_X(_SpecialObjectsNormals, sampler_SpecialObjectsNormals, UnityStereoTransformScreenSpaceTex(input.texcoord)).a;
                float sobelDepth = SobelSampleDepth(input.texcoord, pixelOffset);

                // DEBUG: Visualization of the depth sobel effect.
                // return float4(sobelDepth.xxx, 1.0);

                // Sobel based on both object depth and normals.
                float4 sobelDepthAndNormals = SobelSampleNormalsAndDepth(input.texcoord, pixelOffset);
                float sobelEffect = sobelDepthAndNormals.x + sobelDepthAndNormals.y + sobelDepthAndNormals.z + sobelDepthAndNormals.w; 
                sobelEffect = pow(saturate(sobelEffect) * _SobelEffectMultiplier, _SobelEffectBias);

                // DEBUG: Visualization of the depth sobel effect.
                // return float4(sobelEffect.xxx, 1.0);

                float3 outlineColor = lerp(cameraColor, _OutlineColor.xyz, saturate(sobelEffect) * _OutlineVisibility);
                return float4(outlineColor, 1.0);
            }

            ENDHLSL
        }        
    }
}