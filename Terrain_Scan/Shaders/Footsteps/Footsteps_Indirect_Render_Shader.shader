Shader "GameDevBuddies/Footsteps_Indirect_Render_Shader"
{
    Properties
    {
        _Footsteps_Shape_Texture ("Footsteps Shape Texture", 2D) = "white" {}
        _Footsteps_Color ("Footsteps Color", Color) = (0,0,0,0)
        [HDR] _Footsteps_Highlight_Color ("Footsteps Highlight Color", Color) = (0,0,0,0)
        _Highlight_Tiling ("Highlight Tiling", Vector) = (400, 400, 0, 0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "DisableBatching" = "True" 
        }

        Pass
        {
            Name "Footsteps"
            Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawArgs
            #include "UnityIndirect.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            #pragma vertex FootstepsVertexFunction
            #pragma fragment FootstepsFragmentFunction
            #pragma multi_compile_instancing

            struct VertexInputData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutputData
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float highlightPercentage: TEXCOORD1;
                float4 screenPos: TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };   

            struct FootstepInfo 
            {
                float4 Rotation;
                float3 Position;
                float HighlightPercentage;
                int IsRightFoot;
            };

            // Buffer containing information for every footstep.
            StructuredBuffer<FootstepInfo> _Footsteps;

            // Shape Texture Settings.
            sampler2D _Footsteps_Shape_Texture;
            float4 _Footsteps_Shape_Texture_ST;

            // Other.
            float4 _Footsteps_Color;
            float3 _Footsteps_Highlight_Color;
            float2 _Highlight_Tiling;

            float4x4 CreateRotationMatrix(float4 quaternion)
            {
                // Precalculate coordinate products
                float x = quaternion.x * 2.0F;
                float y = quaternion.y * 2.0F;
                float z = quaternion.z * 2.0F;
                float xx = quaternion.x * x;
                float yy = quaternion.y * y;
                float zz = quaternion.z * z;
                float xy = quaternion.x * y;
                float xz = quaternion.x * z;
                float yz = quaternion.y * z;
                float wx = quaternion.w * x;
                float wy = quaternion.w * y;
                float wz = quaternion.w * z;

                // Calculate 3x3 matrix from orthonormal basis
                float4x4 m;
                m._m00 = 1.0f - (yy + zz); m._m10 = xy + wz; m._m20 = xz - wy; m._m30 = 0.0F;
                m._m01 = xy - wz; m._m11 = 1.0f - (xx + zz); m._m21 = yz + wx; m._m31 = 0.0F;
                m._m02 = xz + wy; m._m12 = yz - wx; m._m22 = 1.0f - (xx + yy); m._m32 = 0.0F;
                m._m03 = 0.0F; m._m13 = 0.0F; m._m23 = 0.0F; m._m33 = 1.0F;
                return m;
            }

            float mod(float x, float y)
            {
                return x - y * floor(x/y);
            }

            float4 lookRotation(float3 forward) 
            {
                float3 t1 = normalize(cross(float3(0, 1, 0), forward));
                float3x3 m = float3x3(t1, cross(forward, t1), forward);

                float3 u = float3(m._m00, m._m01, m._m02);
                float3 v = float3(m._m10, m._m11, m._m12);
                float3 w = float3(m._m20, m._m21, m._m22);

                uint u_sign = (asuint(u.x) & 0x80000000);
                float t = v.y + asfloat(asuint(w.z) ^ u_sign);
                uint4 u_mask = uint4(((int)u_sign >> 31).xxxx);
                uint4 t_mask = uint4((asint(t) >> 31).xxxx);

                float tr = 1.0f + abs(u.x);

                uint4 sign_flips = uint4(0x00000000, 0x80000000, 0x80000000, 0x80000000) ^ (u_mask & uint4(0x00000000, 0x80000000, 0x00000000, 0x80000000)) ^ (t_mask & uint4(0x80000000, 0x80000000, 0x80000000, 0x00000000));

                float4 value = float4(tr, u.y, w.x, v.z) + asfloat(asuint(float4(t, v.x, u.z, w.y)) ^ sign_flips);   // +---, +++-, ++-+, +-++

                value = asfloat((asuint(value) & ~u_mask) | (asuint(value.zwxy) & u_mask));
                value = asfloat((asuint(value.wzyx) & ~t_mask) | (asuint(value) & t_mask));
                value = normalize(value);
                return value;
            }
            
            VertexOutputData FootstepsVertexFunction (VertexInputData inputData, uint svInstanceID: SV_InstanceID)
            {
                InitIndirectDrawArgs(0);

                VertexOutputData o;

                UNITY_SETUP_INSTANCE_ID(inputData);
                UNITY_TRANSFER_INSTANCE_ID(inputData, o);

                uint instanceID = GetIndirectInstanceID(svInstanceID);
                FootstepInfo footstepInfo = _Footsteps[instanceID];
                float3 vertexPos = inputData.vertex.xyz;
                
                // Scaling the vertex position towards mesh origin based on the size of the footstep.
                // The values were found out during manual testing in the editor.
                vertexPos = float3(vertexPos.x * 0.2, vertexPos.y * 0.4, vertexPos.z * 0.2);
                // Rotating the icon based on the provided rotation.
                vertexPos = mul(CreateRotationMatrix(footstepInfo.Rotation), float4(vertexPos, 1.0)).xyz;                
                // Re-positioning the footstep to the correct world position.
                vertexPos += footstepInfo.Position;          

                float2 inputUV = float2(inputData.uv.x + footstepInfo.IsRightFoot * 0.5, inputData.uv.y);    

                o.positionCS = TransformObjectToHClip(vertexPos);
                o.uv = TRANSFORM_TEX(inputData.uv, _Footsteps_Shape_Texture);
                o.uv.x += (1.0 - footstepInfo.IsRightFoot) * 0.5;
                o.highlightPercentage = footstepInfo.HighlightPercentage;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }
            
            float4 FootstepsFragmentFunction (VertexOutputData vertexOutputData) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(vertexOutputData);

                float footstepShape = tex2D(_Footsteps_Shape_Texture, vertexOutputData.uv).r;

                // Normal footsteps color, used when the terrain scan is inactive or the footstep is
                // outside its activity range.
                float3 normalFootstepColor = _Footsteps_Color.rgb;

                // Highlighted footstep color, used when footstep is inside the terrain scan activity range.
                float4 screenPos = vertexOutputData.screenPos / vertexOutputData.screenPos.w;
                float2 tiledScreenPos = frac(screenPos * _Highlight_Tiling);
                float highlightMinimum = min(tiledScreenPos.x, tiledScreenPos.y);
                float3 highlightedFootstepColor = highlightMinimum * _Footsteps_Highlight_Color;

                float4 footstepColor = lerp(float4(normalFootstepColor.xyz, footstepShape), float4(highlightedFootstepColor.xyz, footstepShape * highlightMinimum), vertexOutputData.highlightPercentage);
                return footstepColor;
            }
            ENDHLSL
        }
    }
}
