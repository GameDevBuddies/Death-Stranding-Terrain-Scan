Shader "GameDevBuddies/Terrain_Scan_Icons_Shader"
{
    Properties
    {
        [Header(Shape Texture Settings)] [Space]
        _IconShapeTexture ("Icon Shape Texture", 2D) = "white" {}
        _IconShapeTextureColumnCount ("Icon Shape Texture Column Count", Range(1, 10)) = 3
        _IconShapeTextureRowCount ("Icon Shape Texture Row Count", Range(1, 10)) = 3

        [Header(Color Texture Settings)] [Space]
        _IconColorMapTexture ("Icon Color Map Texture", 2D) = "white" {}
        _IconColorMapTextureColumnCount ("Icon Color Map Texture Column Count", Range(1, 10)) = 3

        [Header(Other Settings)] [Space]
        _IconColorIntensity ("Icon Color Intensity", Range(0.0, 10.0)) = 5.0

        [Header(Unwalkable Icons Settings)] [Space]
        _HorizontalSizeExpansionMultiplier ("Horizontal Size Expansion Multiplier", Range(1.0, 128.0)) = 64.0
        _ExpandedAspectRatio ("Expanded Aspect Ratio", Range(1.0, 10.0)) = 8.0
        _GlowTexture ("Glow Texture", 2D) = "white" {}
        _GlowAnimationDuration ("Glow Animation Duration", Range(0.001, 2.0)) = 0.33        
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
            Name "Terrain Scan Icons"
            Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawArgs
            #include "UnityIndirect.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "./Terrain_Scan_Icons_Shared.hlsl"

            #pragma vertex TerrainScanIconsVertexFunction
            #pragma fragment TerrainScanIconsFragmentFunction
            #pragma multi_compile_instancing

            struct VertexInputData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutputData
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                uint2 shapeAndColor: TEXCOORD1;

                // X = icon opacity, Y = spawn animation percentage, 
                // Z, W = current size expansion multiplier vector
                float4 additionalIconData: TEXCOORD2;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Buffer containing information for every icon, specifying it's rotation, 
            // position, size, type and danger level.
            StructuredBuffer<TerrainIconInfo> _TerrainIconsData;     

            // Shape Texture Settings.
            sampler2D _IconShapeTexture;
            float4 _IconShapeTexture_ST;
            float _IconShapeTextureColumnCount;
            float _IconShapeTextureRowCount; 

            // Color Map Texture Settings.
            sampler2D _IconColorMapTexture;
            float4 _IconColorMapTexture_ST;
            float _IconColorMapTextureColumnCount;

            // Other Settings.
            float _IconColorIntensity;
            float4 _MainCameraWorldPosition;
            float4 _TerrainScanOrigin;

            // Unwalkable Icons Settings.
            float _HorizontalSizeExpansionMultiplier;
            float _ExpandedAspectRatio;
            float _GlowAnimationDuration;
            sampler2D _GlowTexture;
            float4 _GlowTexture_ST;
            float _CurrentTime;

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

            float ExtractIconShapeAlpha(float iconShapeIndex, float2 originalUV)
            {
                // Round up the column/row count of the shape flip book to whole numbers.
                float columnCount = floor(_IconShapeTextureColumnCount);
                float rowCount = floor(_IconShapeTextureRowCount);

                // Convert icon shape index into row/column indices for sampling the flipbook.
                float columnIndex = mod(iconShapeIndex, columnCount);
                float rowIndex = floor(iconShapeIndex / rowCount);

                // Calculate the range for the X-coordinate sampling.
                float columnRangeMultiplier = 1.0 / columnCount;
                float xCoordMin = (float)columnIndex * columnRangeMultiplier;
                float xCoordMax = ((float)columnIndex + 1) * columnRangeMultiplier; 

                // Calculate the range for the Y-coordinate sampling.
                float rowRangeMultiplier = 1.0 / rowCount;
                float yCoordMin = (float)rowIndex * rowRangeMultiplier;
                float yCoordMax = ((float)rowIndex + 1) * rowRangeMultiplier;
                
                // Calculate the UV coordinates for sampling the shape flipbook correctly.
                float2 iconShapeUV;
                iconShapeUV.x = lerp(xCoordMin, xCoordMax, originalUV.x);
                iconShapeUV.y = 1.0 - lerp(yCoordMin, yCoordMax, 1.0 - originalUV.y);

                // Sample the shape texture and output the result.
                return tex2D(_IconShapeTexture, iconShapeUV).r;
            }

            float3 ExtractIconColor(float iconColorMapIndex, float2 originalUV)
            {
                // Calculating the column of the icon based on the color map index.
                float columnCount = floor(_IconColorMapTextureColumnCount);
                float columnIndex = mod(iconColorMapIndex, columnCount);

                // Calculating the X-coordinate range based on the index.
                float columnRangeMultiplier = 1.0 / columnCount;
                float xCoordMin = (float)columnIndex * columnRangeMultiplier;
                float xCoordMax = ((float)columnIndex + 1) * columnRangeMultiplier; 

                // Calculate the UV coordinates for sampling the color map flip book correctly.
                float2 iconColorMapUV;
                iconColorMapUV.x = lerp(xCoordMin, xCoordMax, originalUV.x);
                // Reading from the middle of the texture (height-wise).
                iconColorMapUV.y = 0.5;

                // Sample the shape texture and output the result.
                return tex2D(_IconColorMapTexture, iconColorMapUV).rgb;
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
            
            VertexOutputData TerrainScanIconsVertexFunction (VertexInputData inputData, uint svInstanceID: SV_InstanceID)
            {
                InitIndirectDrawArgs(0);

                VertexOutputData o;

                UNITY_SETUP_INSTANCE_ID(inputData);
                UNITY_TRANSFER_INSTANCE_ID(inputData, o);

                uint instanceID = GetIndirectInstanceID(svInstanceID);
                TerrainIconInfo terrainIconInfo = _TerrainIconsData[instanceID];
                float3 vertexPos = inputData.vertex.xyz;

                // Calculate the rotation quaternion based on the position of the camera and the 
                // world position of the icon.
                float3 iconLookVector = normalize(((_MainCameraWorldPosition.xyz - _TerrainScanOrigin.xyz) - terrainIconInfo.Position));
                float4 iconRotation = lookRotation(iconLookVector);

                // Calculating the spawn animation percentage. This is only used for the unwalkable terrain icons, 
                // all others have this set to 1.0 so it would be discarded in the fragment shader.
                float spawnAnimationPercentage = 1.0;
                float2 sizeMultiplier = float2(1.0, 1.0);

                // Scaling of the dangerous terrain icons so that they could show glowing overlay.
                if(terrainIconInfo.ShapeID == 1 && terrainIconInfo.DangerIconSpawnTimeRecorded > 0) 
                {
                    spawnAnimationPercentage = saturate((_CurrentTime - terrainIconInfo.DangerIconSpawnTime) / _GlowAnimationDuration);
                    float2 glowSizeMultiplier = float2(_HorizontalSizeExpansionMultiplier, _HorizontalSizeExpansionMultiplier / _ExpandedAspectRatio);                    
                    sizeMultiplier = lerp(glowSizeMultiplier, float2(1.0, 1.0), spawnAnimationPercentage);
                    vertexPos.x *= sizeMultiplier.x;
                    vertexPos.y *= sizeMultiplier.y;
                }
                
                // Scaling the vertex position towards mesh origin based on the size of the icon.
                vertexPos *= terrainIconInfo.Size;

                // Rotating the icon based on the provided rotation.
                vertexPos = mul(CreateRotationMatrix(iconRotation), float4(vertexPos, 1.0)).xyz;
                
                // Re-positioning the icon to the correct world position.
                vertexPos += terrainIconInfo.Position;                

                o.vertex = TransformObjectToHClip(vertexPos);
                o.uv = TRANSFORM_TEX(inputData.uv, _IconShapeTexture);
                o.shapeAndColor = uint2(terrainIconInfo.ShapeID, terrainIconInfo.ColorID);
                o.additionalIconData = float4(terrainIconInfo.Opacity, spawnAnimationPercentage, sizeMultiplier.xy);
                return o;
            }
            
            float4 TerrainScanIconsFragmentFunction (VertexOutputData vertexOutputData) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(vertexOutputData);

                // Reading the shape and color indices for this icon.
                float iconShapeIndex = (float)vertexOutputData.shapeAndColor.x;
                float iconColorIndex = (float)vertexOutputData.shapeAndColor.y;

                // Alpha masking the icon based on the saturation.
                float2 originalUV = vertexOutputData.uv;
                float2 sizeMultiplier = vertexOutputData.additionalIconData.zw;
                float2 alphaUV = saturate((originalUV * sizeMultiplier) - 0.5 * (sizeMultiplier - float2(1,1)));
                float iconShapeAlpha = ExtractIconShapeAlpha(iconShapeIndex, alphaUV);

                // Color sampling based on the danger level the icon represents.
                float3 iconBaseColor = ExtractIconColor(iconColorIndex, originalUV);

                // Optionally sampling the glow texture if the spawn animation is active. 
                // (Only happening for the unwalkable terrain icons.)
                float spawnAnimationPercentage = vertexOutputData.additionalIconData.y;
                if(spawnAnimationPercentage < 1.0)
                {
                    float glowIconShape = tex2D(_GlowTexture, originalUV).r;
                    iconShapeAlpha = lerp(glowIconShape, iconShapeAlpha, pow(spawnAnimationPercentage, 6.0));
                }

                // Getting the final icon color based on shape and danger level color.
                float4 iconColor = float4(iconBaseColor.rgb, iconShapeAlpha);

                float iconOpacity = vertexOutputData.additionalIconData.x;
                iconColor.a *= iconOpacity;

                // Adjusting the intensity of the icon color so that it stands out from the environment.
                iconColor.rgb *= pow(2, _IconColorIntensity * iconOpacity);

                return iconColor;
            }
            ENDHLSL
        }
    }
}