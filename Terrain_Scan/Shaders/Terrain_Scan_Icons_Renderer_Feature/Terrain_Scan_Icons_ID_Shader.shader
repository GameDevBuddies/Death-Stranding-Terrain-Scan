Shader "GameDevBuddies/Terrain_Scan_Icons_ID_Shader"
{
    Properties
    {
        _ObjectTypeID("Object Type ID", Int) = 0.0
        _VisualizationColor("Debug Visualization Color", Color) = (1,1,1,1)
    }

    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct VertexInput {
            float4 localSpacePosition : POSITION;

            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct VertexOutput {
            float4 clipSpacePosition : SV_POSITION;

            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        VertexOutput TerrainIconsIdVertexFunction(VertexInput input) {
            VertexOutput output;
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);

            output.clipSpacePosition = TransformObjectToHClip(input.localSpacePosition.xyz);

            return output;
        }
    ENDHLSL
    
    SubShader
    {
        Pass 
        {
            Name "Terrain_Icons_ID"
            Blend Zero One, One Zero

            Tags 
            {
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
                "LightMode" = "Terrain_Icons_ID"
            }

            HLSLPROGRAM

            #pragma vertex TerrainIconsIdVertexFunction
            #pragma fragment TerrainIconsIdFragmentFunction            

            CBUFFER_START(UnityPerMaterial)
                int _ObjectTypeID;
            CBUFFER_END

            float EncodeIdUNORM() 
            {
                return _ObjectTypeID / 255.0;
            }

            float EncodeIdSNORM() 
            {
                return _ObjectTypeID / 127.0;
            }

            float4 TerrainIconsIdFragmentFunction(VertexOutput input) : SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);
                
                return float4(0,0,0, EncodeIdSNORM());
            }

            ENDHLSL
        }        

        Pass 
        {
            Name "Default Scene View"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            Tags 
            {
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
            }

            HLSLPROGRAM

            #pragma vertex TerrainIconsIdVertexFunction
            #pragma fragment TerrainIconsIdFragmentFunction

            CBUFFER_START(UnityPerMaterial)
                float4 _VisualizationColor;
            CBUFFER_END

            float4 TerrainIconsIdFragmentFunction(VertexOutput input) : SV_TARGET{
                UNITY_SETUP_INSTANCE_ID(input);
                
                return _VisualizationColor;
            }

            ENDHLSL
        } 
    }
}