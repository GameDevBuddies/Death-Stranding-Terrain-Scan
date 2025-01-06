#ifndef TERRAIN_SCAN_ICONS_RENDERER_INCLUDED
#define TERRAIN_SCAN_ICONS_RENDERER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutputNormalDepth TerrainIconsNormalDepthVertexFunction(VertexInputNormalDepth input) {
	VertexOutputNormalDepth output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

	VertexNormalInputs normalInput = GetVertexNormalInputs(input.normal, input.tangentOS);

	output.clipSpacePosition = TransformObjectToHClip(input.localSpacePosition.xyz); 
	output.normal = float3(NormalizeNormalPerVertex(normalInput.normalWS));

	return output;
}

float4 TerrainIconsNormalDepthFragmentFunction(VertexOutputNormalDepth input) : SV_TARGET{
	UNITY_SETUP_INSTANCE_ID(input);
    
	return float4(normalize(input.normal), 1);
}

#endif // TERRAIN_ICONS_RENDERER_INCLUDED