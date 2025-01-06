Shader "GameDevBuddies/Special_Objects_Outline_Normals_Shader"
{
    Properties
    {
    }
    
    SubShader
    {
        Tags 
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Blend One Zero

        Pass 
        {
            Name "Terrain_Icons_Normal_Depth"

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex TerrainIconsNormalDepthVertexFunction
            #pragma fragment TerrainIconsNormalDepthFragmentFunction

            // Rendering normals and depth.
            struct VertexInputNormalDepth {
                float4 localSpacePosition : POSITION;
                float4 normal : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv: TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutputNormalDepth {
                float4 clipSpacePosition : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 worldPosition: TEXCOORD1;
                float2 uv: TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            inline float LinearEyeDepth( float z )
            {
                return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
            }

            VertexOutputNormalDepth TerrainIconsNormalDepthVertexFunction(VertexInputNormalDepth input) {
                VertexOutputNormalDepth output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs attributes = GetVertexPositionInputs(input.localSpacePosition.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal.xyz, input.tangentOS);

                output.clipSpacePosition = TransformObjectToHClip(input.localSpacePosition.xyz); 
                output.normal = float3(NormalizeNormalPerVertex(normalInput.normalWS));
                output.worldPosition = attributes.positionWS.xyz;
                output.uv = input.uv;

                return output;
            }

            float4 TerrainIconsNormalDepthFragmentFunction(VertexOutputNormalDepth input) : SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);

                float depthTextureSample = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, UnityStereoTransformScreenSpaceTex(input.uv)).r;
                float depth = LinearEyeDepth(depthTextureSample);
                return float4(normalize(input.normal), depth);
            }

            ENDHLSL
        }        
    }
}