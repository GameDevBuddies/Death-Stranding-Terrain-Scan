Shader "GameDevBuddies/Terrain_Scan_Normals_Depth_Shader"
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

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutputNormalDepth {
                float4 clipSpacePosition : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 worldPosition: TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            VertexOutputNormalDepth TerrainIconsNormalDepthVertexFunction(VertexInputNormalDepth input) {
                VertexOutputNormalDepth output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs attributes = GetVertexPositionInputs(input.localSpacePosition.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal.xyz, input.tangentOS);

                output.clipSpacePosition = TransformObjectToHClip(input.localSpacePosition.xyz); 
                output.normal = float3(NormalizeNormalPerVertex(normalInput.normalWS));
                output.worldPosition = attributes.positionWS.xyz;

                return output;
            }

            float4 TerrainIconsNormalDepthFragmentFunction(VertexOutputNormalDepth input) : SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);
                
                return float4(normalize(input.normal), 0.0);
            }

            ENDHLSL
        }        
    }
}