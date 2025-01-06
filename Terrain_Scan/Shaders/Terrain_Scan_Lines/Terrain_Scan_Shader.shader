Shader "GameDevBuddies/Terrain_Scan_Shader"
{
    Properties
    {
        [Header(Terrain Scan Settings)] [Space]
        _Terrain_Scan_Origin("Terrain Scan Origin", Vector) = (0, 0, 0, 0)
        _Terrain_Scan_Direction_XZ("Terrain Scan Direction (XZ)", Vector) = (0.5, 0.5, 0, 0)
        _Terrain_Scan_Range("Terrain Scan Range", Range(0.1, 2000)) = 100
        _Terrain_Scan_Arc_Angle("Terrain Scan Arc Angle", Range(0, 180)) = 120        

        [Header(Edge Gradient)] [Space]
        _Edge_Gradient_Start_Distance_Percentage("Edge Gradient Start Distance (Percentage)", Range(0.0, 1.0)) = 0.9
        _Edge_Gradient_Falloff("Edge Gradient Falloff", Range(1.0, 10.0)) = 3.0
        [HDR]_Edge_Gradient_Start_Color("Edge Gradient Start Color", Color) = (0, 33.63058, 64, 0)
        [HDR]_Edge_Gradient_End_Color("Edge Gradient End Color", Color) = (0.03931776, 0.3997108, 1.786163, 0)

        [Header(Scan Lines)] [Space]
        _Outline_Thickness("Outline Thickness", Range(1.0, 5.0)) = 1.0
        _Distance_Between_Scan_Lines("Distance Between Scan Lines", Range(0.5, 10)) = 2.5
        _Scan_Lines_Min_Visible_Distance("Scan Lines Min Visible Distance", Range(0.0, 10.0)) = 3.0
        _Scan_Lines_Visibility_Falloff("Scan Lines Visibility Falloff", Range(0.0, 10.0)) = 1.0
        [HDR]_Scan_Line_Color("Scan Line Color", Color) = (2.389936, 4.074151, 8, 0)

        [Header(Ground Darken)] [Space]
        _Ground_Darken_Edge_Offset("Ground Darken Edge Offset", Range(0, 1)) = 0.25
        _Ground_Darken_Edge_Falloff("Ground Darken Edge Falloff", Range(0.000001, 2.0)) = 0.5 
        _Ground_Darken_Color("Ground Darken Color", Color) = (0, 0, 0, 0)

        [Header(Ground Darken Circle)] [Space]
        _Dark_Circle_Size("Dark Circle Size", Range(0, 10)) = 3
        _Dark_Circle_Color("Dark Circle Color", Color) = (0, 0, 0, 0)
        _Dark_Circle_Visibility("Dark Circle Visibility", Range(0.0, 1.0)) = 1.0

        [Header(Side Edge Soften)] [Space]
        _Side_Edge_Soften_Arc_Angle("Side Edges Soften Arc Angle", Range(0, 180)) = 100
        _Side_Edge_Soften_Falloff("Side Edges Soften Falloff", Range(1.0, 10.0)) = 2.0

        [Header(Global Opacity Control)] [Space]
        _Global_Visibility("Global Visibility", Range(0, 1)) = 1.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalFullscreenSubTarget"
        }
        Pass
        {
            Name "DrawProcedural"
        
            Cull Off
            Blend Off
            ZTest Off
            ZWrite Off
            
            HLSLPROGRAM
            
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            
            #define FULLSCREEN_SHADERGRAPH
            
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_VERTEXID
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD1            
            #define REQUIRE_DEPTH_TEXTURE
            #define REQUIRE_NORMAL_TEXTURE
            #define SHADERPASS SHADERPASS_DRAWPROCEDURAL
            
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "./CustomBlendingFunctions.hlsl"
            
            struct Attributes
            {
                uint vertexID : VERTEXID_SEMANTIC;
            };

            struct SurfaceDescriptionInputs
            {
                float3 WorldSpacePosition;
                float4 ScreenPosition;
                float2 NDCPosition;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0;
                float4 texCoord1;
            };

            struct VertexDescriptionInputs
            {
            };

            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0 : INTERP0;
                float4 texCoord1 : INTERP1;
            };

            struct SurfaceDescription
            {
                float3 BaseColor;
                float Alpha;
            };
        
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                ZERO_INITIALIZE(PackedVaryings, output);

                output.positionCS = input.positionCS;
                output.texCoord0.xyzw = input.texCoord0;
                output.texCoord1.xyzw = input.texCoord1;
                
                return output;
            }
            
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;

                output.positionCS = input.positionCS;
                output.texCoord0 = input.texCoord0.xyzw;
                output.texCoord1 = input.texCoord1.xyzw;
                
                return output;
            }
            
            CBUFFER_START(UnityPerMaterial)
            // Terrain Scan Main Variables.
            float3 _Terrain_Scan_Origin;
            float3 _Terrain_Scan_Direction_XZ;
            float _Terrain_Scan_Range;
            float _Terrain_Scan_Arc_Angle;
            float _Global_Visibility;

            // Edge Gradient.
            float _Edge_Gradient_Start_Distance_Percentage;
            float _Edge_Gradient_Falloff;
            float4 _Edge_Gradient_Start_Color;
            float4 _Edge_Gradient_End_Color;

            // Scan Lines.
            float _Outline_Thickness;
            float _Distance_Between_Scan_Lines;
            float _Scan_Lines_Min_Visible_Distance;
            float _Scan_Lines_Visibility_Falloff;
            float4 _Scan_Line_Color;

            // Ground Darken.
            float _Ground_Darken_Edge_Offset;
            float _Ground_Darken_Edge_Falloff;
            float4 _Ground_Darken_Color;

            // Ground Dark Circle.
            float _Dark_Circle_Size;
            float4 _Dark_Circle_Color;
            float _Dark_Circle_Visibility;

            // Side Edges Softening.
            float _Side_Edge_Soften_Arc_Angle;
            float _Side_Edge_Soften_Falloff;
            CBUFFER_END        
        
            // Global property required for fullscreen effects.       
            float _FlipY;
            
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 SampleColorTexture(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
            }

            float2 Remap(float2 inputValue, float2 inputMinMax, float2 outputMinMax)
            {
                float remapMultiplier = (outputMinMax.y - outputMinMax.x) / (inputMinMax.y - inputMinMax.x);
                return outputMinMax.xx + (inputValue - inputMinMax.xx) * remapMultiplier.xx;
            }

            float Remap(float inputValue, float fromMin, float fromMax, float toMin, float toMax)
            {
                float remapMultiplier = (toMax - toMin) / (fromMax - fromMin);
                return toMin + (inputValue - fromMin) * remapMultiplier;
            }

            float3 CalculateWorldPosition(float2 screenPosition, float rawDepth)
            {
                // Remapping from [0, 1] range into [-1, 1] range.
                float2 remappedScreenPosition = Remap(screenPosition, float2(0, 1), float2(-1, 1));
                // Flipping Y coordinate to match correct Y direction.
                remappedScreenPosition.y *= -1.0;

                // Transformation from clip space into view space with inverse projection matrix.
                float4 clipSpacePosition = float4(remappedScreenPosition, rawDepth, 1.0);
                float4 viewSpacePosition = mul(UNITY_MATRIX_I_P, clipSpacePosition);
                viewSpacePosition /= viewSpacePosition.w;

                // Getting the world position of the pixel.
                float3 worldSpacePosition = GetAbsolutePositionWS(TransformViewToWorld(viewSpacePosition.xyz));
                return worldSpacePosition;
            }

            float3 CalculateWorldPosition(float2 screenPosition)
            {
                float rawDepth = SampleSceneDepth(screenPosition);
                float3 worldSpacePosition = CalculateWorldPosition(screenPosition, rawDepth);
                return worldSpacePosition;
            }

            float SobelSampleDepth(float2 uv, float2 pixelOffset)
            {
                float2 horizontalOffset = float2(pixelOffset.x, 0);
                float2 verticalOffset = float2(0, pixelOffset.y);

                // Distances of each pixel to the origin of the terrain scan effect.
                float centerDistanceToOrigin = distance(CalculateWorldPosition(uv),                    _Terrain_Scan_Origin);
                float leftDistanceToOrigin   = distance(CalculateWorldPosition(uv - horizontalOffset), _Terrain_Scan_Origin);
                float rightDistanceToOrigin  = distance(CalculateWorldPosition(uv + horizontalOffset), _Terrain_Scan_Origin);
                float upDistanceToOrigin     = distance(CalculateWorldPosition(uv + verticalOffset),   _Terrain_Scan_Origin);
                float downDistanceToOrigin   = distance(CalculateWorldPosition(uv - verticalOffset),   _Terrain_Scan_Origin);

                // Comparing intervals to get the "lines" effect.
                float center = floor(centerDistanceToOrigin / _Distance_Between_Scan_Lines);
                float left = floor(leftDistanceToOrigin / _Distance_Between_Scan_Lines);
                float right = floor(rightDistanceToOrigin / _Distance_Between_Scan_Lines);
                float up = floor(upDistanceToOrigin / _Distance_Between_Scan_Lines);
                float down = floor(downDistanceToOrigin / _Distance_Between_Scan_Lines);

                return (center - left) + (center - right) + (center - up) + (center - down);
            }

            float InverseLerp(float minValue, float maxValue, float value)
            {
                return saturate((value - minValue) / (maxValue - minValue));
            }

            float3 TerrainScanFragmentFunction(SurfaceDescriptionInputs IN){
                // Screen UV in range [0, 1].
                float2 screenPosition = IN.NDCPosition.xy;
                float3 originalColor = SampleColorTexture(screenPosition).xyz;

                // DEBUG VISUALIATION: ORIGINAL COLOR
                // return originalColor;

                // --- Calculate world position of the pixel from its depth. ---
                float rawDepth = SampleSceneDepth(screenPosition);
                float3 worldSpacePosition = CalculateWorldPosition(screenPosition, rawDepth);

                // DEBUG VISUALIATION: DEPTH
                // return saturate(Linear01Depth(rawDepth, _ZBufferParams));

                // Making sure that the skybox doesn't get into calculations.
                if(rawDepth <= 0.0)
                {
                    return originalColor.xyz;
                }                                            

                // -- Calculate distance from the origin of the terrain scan effect. ---
                float rawDistanceFromOrigin = distance(_Terrain_Scan_Origin, worldSpacePosition);
                float rawDistanceFromEdge = _Terrain_Scan_Range - rawDistanceFromOrigin;
                // If the distance is greater than the current Terrain Scan Range, it will be 0.
                float distanceFromOriginMask = step(0, rawDistanceFromEdge);
                float distanceFromOrigin01 = distanceFromOriginMask * rawDistanceFromOrigin / _Terrain_Scan_Range;

                // DEBUG VISUALIZATION: DISTANCE FROM ORIGIN (0-1)
                // return distanceFromOriginMask;

                // -- Calculate the circle sector (arc) for the terrain scan effect. ---
                float2 directionFromOriginXZ = normalize(worldSpacePosition.xz - _Terrain_Scan_Origin.xz);
                float directionCorrelation = dot(normalize(_Terrain_Scan_Direction_XZ).xz, directionFromOriginXZ);
                float halfMaxAngle = radians(0.5 * _Terrain_Scan_Arc_Angle);
                float angle01 = InverseLerp(cos(halfMaxAngle), 1.0, directionCorrelation);
                float angleMask = step(0.0, angle01);
                
                // DEBUG VISUALIZATION: ANGLE MASK
                // return angleMask;

                // DEBUG VISUALIZATION: ANGLE AND DISTANCE MASK
                // return angleMask * angle01 * distanceFromOriginMask;

                // -- Darkening effect. -- 
                float darkeningEffect = 1.0 - pow(1.0 - saturate(distanceFromOrigin01 + _Ground_Darken_Edge_Offset), _Ground_Darken_Edge_Falloff) 
                    / pow(saturate(1.0 - _Ground_Darken_Edge_Offset), _Ground_Darken_Edge_Falloff);
                float3 darkenedOriginalColor = Blend_Darken(originalColor, _Ground_Darken_Color.rgb, 
                    angleMask * distanceFromOriginMask * darkeningEffect * _Ground_Darken_Color.a);

                // DEBUG VISUALIZATION: DARKENING EFFECT
                // return darkenedOriginalColor;

                // -- Edge Gradient Effect. --
                float edgeGradientStartDistance = _Edge_Gradient_Start_Distance_Percentage * _Terrain_Scan_Range;
                float edgeGradient01 = pow(saturate(InverseLerp(edgeGradientStartDistance, _Terrain_Scan_Range, rawDistanceFromOrigin)), _Edge_Gradient_Falloff);
                float3 edgeGradientColor = lerp(_Edge_Gradient_Start_Color, _Edge_Gradient_End_Color, edgeGradient01).xyz;    
                float3 darkenedEdgeGradientColor = Blend_Lighten(darkenedOriginalColor, edgeGradientColor, edgeGradient01 * angleMask * distanceFromOriginMask);            

                // DEBUG VISUALIZATION: EDGE GRADIENT MASK
                // return edgeGradient01 * angleMask * distanceFromOriginMask;

                // DEBUG VISUALIZATION: EDGE GRADIENT COLOR
                // float3 originalColorWithEdgeGradient = Blend_Lighten(originalColor, edgeGradientColor, edgeGradient01);
                // return originalColorWithEdgeGradient * angleMask * distanceFromOriginMask;

                // DEBUG VISUALIZATION: DARKENED EDGE GRADIENT COLOR
                // return lerp(originalColor, darkenedEdgeGradientColor, angleMask * distanceFromOriginMask);

                // -- Softening the edges of the angle area to appear smoother. --
                float halfSideEdgesAngle = radians(0.5 * min(_Side_Edge_Soften_Arc_Angle, _Terrain_Scan_Arc_Angle));
                float sideEdgesSoftenAngleMask = saturate(InverseLerp(cos(halfMaxAngle), cos(halfSideEdgesAngle), directionCorrelation));
                sideEdgesSoftenAngleMask = saturate(pow(sideEdgesSoftenAngleMask, _Side_Edge_Soften_Falloff));                
                float3 softenedDarkenedColor = lerp(originalColor, darkenedEdgeGradientColor, sideEdgesSoftenAngleMask * distanceFromOriginMask);

                // DEBUG VISUALIZATION: SOFTEN SIDE EDGES MASK
                // return sideEdgesSoftenAngleMask * angleMask * distanceFromOriginMask;

                // DEBUG VISUALIZATION: SOFTEN SIDE EDGES DARKEN EFFECT WITH EDGE GRADIENT
                // return softenedDarkenedColor;

                // -- Sobel Outline Effect. --
                float2 pixelOffset = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y) * _Outline_Thickness;
                float sobelOutlineMask = saturate(SobelSampleDepth(screenPosition, pixelOffset) / _Outline_Thickness);

                // DEBUG VISUALIATION: SOBEL OUTLINE
                // return sobelOutlineMask; 

                // -- Scan lines visibility mask. --
                float scanLinesVisibilityMultiplier = pow(saturate(InverseLerp(_Scan_Lines_Min_Visible_Distance, _Terrain_Scan_Range, rawDistanceFromOrigin)), _Scan_Lines_Visibility_Falloff);
                float firstLineColorMask = step((ceil(_Terrain_Scan_Range / _Distance_Between_Scan_Lines) - 1) * _Distance_Between_Scan_Lines, rawDistanceFromOrigin);

                // DEBUG VISUALIZATION: SCAN LINES MASK
                // return scanLinesVisibilityMultiplier * sobelOutlineMask * angleMask * distanceFromOriginMask;

                // DEBUG VISUALIZATION: FIRST LINE MASK
                // return firstLineColorMask * sobelOutlineMask * angleMask * distanceFromOriginMask + (1.0 - firstLineColorMask) * 0.2 * sobelOutlineMask * angleMask * distanceFromOriginMask;

                // -- Scan lines color, with the first line being white. --
                float scanLinesInterpolationFactor = sobelOutlineMask * angleMask * distanceFromOriginMask * sideEdgesSoftenAngleMask * scanLinesVisibilityMultiplier;                
                float3 scanLineColor = lerp(_Scan_Line_Color.xyz, float3(1,1,1), firstLineColorMask);
                float3 scanLinesColor = Blend_Lighten(originalColor, scanLineColor, scanLinesInterpolationFactor);
                float3 darkenedEdgeGradientScanLinesColor = Blend_Lighten(softenedDarkenedColor, scanLineColor, scanLinesInterpolationFactor);

                // DEBUG VISUALIZATION: SCAN LINES COLOR
                // return scanLinesColor;

                // DEBUG VISUALIZATION: SCAN LINES COLOR WITH EDGE GRADIENT AND DARKENING
                // return darkenedEdgeGradientScanLinesColor;

                // -- Calculate the outside circle around origin (Visible only if not inside arc). --
                float circleSize = min(_Terrain_Scan_Range, _Dark_Circle_Size);
                float originCircleMask = 1.0 - step(0.0, rawDistanceFromOrigin - circleSize);                
                float originCircleDistance01 = circleSize == 0.0 ? 0.0 : originCircleMask * rawDistanceFromOrigin / circleSize;
                float3 darkenCircleColor = Blend_Darken(originalColor, originalColor * _Dark_Circle_Color.rgb, (1.0 - sideEdgesSoftenAngleMask) * originCircleDistance01 * _Dark_Circle_Color.a);
                float3 circleWithAllOtherEffects = lerp(darkenedEdgeGradientScanLinesColor, darkenCircleColor, (1.0 - sideEdgesSoftenAngleMask) * originCircleMask * _Dark_Circle_Visibility);

                // DEBUG VISUALIZATION: CIRCLE AROUND ORIGIN MASK (OUTSIDE OF THE ARC) [0, 1]
                // return (1.0 - sideEdgesSoftenAngleMask) * pow(originCircleDistance01, 4.0);

                // DEBUG VISUALIZATION: DARKENED CIRCLE WTIH ALL OTHER EFFECTS
                // return circleWithAllOtherEffects;

                float3 finalColor = lerp(originalColor, circleWithAllOtherEffects, _Global_Visibility);
                return finalColor;
            }
        
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                surface.Alpha = 1.0;
                surface.BaseColor = TerrainScanFragmentFunction(IN);
                return surface;                
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);      

                float3 viewDirWS = normalize(input.texCoord1.xyz);
                float linearDepth = LinearEyeDepth(SampleSceneDepth(input.texCoord0.xy), _ZBufferParams);
                float3 cameraForward = -UNITY_MATRIX_V[2].xyz;
                float cameraDistance = linearDepth / dot(viewDirWS, cameraForward);
                float3 positionWS = viewDirWS * cameraDistance + GetCameraPositionWS();            
            
                output.WorldSpacePosition = positionWS;
                output.ScreenPosition = float4(input.texCoord0.xy, 0, 1);
                output.NDCPosition = input.texCoord0.xy;
            
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/Fullscreen/Includes/FullscreenCommon.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/Fullscreen/Includes/FullscreenDrawProcedural.hlsl"
        
            ENDHLSL
        }
    }
    CustomEditor "UnityEditor.Rendering.Fullscreen.ShaderGraph.FullscreenShaderGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}