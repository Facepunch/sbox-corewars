HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "Pixel Material";
}

FEATURES
{
    #include "common/features.hlsl"

	Feature( F_TRANSLUCENT, 0..1, "Rendering")
	Feature( F_IS_VOXEL_BLOCK, 0..1, "Rendering" );
}

MODES
{
    VrForward();
    Depth( "vr_depth_only.vfx" );
    ToolsVis( S_MODE_TOOLS_VIS );
    ToolsWireframe( "vr_tools_wireframe.vfx" );
	ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" );
}

COMMON
{
	#include "common/shared.hlsl"
	
    #define CUSTOM_TEXTURE_FILTERING
    SamplerState TextureFiltering < Filter( POINT ); MaxAniso( 8 ); >;

	#define BLEND_MODE_ALREADY_SET
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"
	
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
    #include "common/pixel.hlsl"
	
	StaticCombo( S_TRANSLUCENT, F_TRANSLUCENT, Sys( PC ) );
	StaticCombo( S_IS_VOXEL_BLOCK, F_IS_VOXEL_BLOCK, Sys( ALL ) );

	RenderState(BlendEnable, S_TRANSLUCENT);
	
	SamplerState g_sPointSampler < Filter( POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
	
	#if S_IS_VOXEL_BLOCK
	float4 g_vVoxelLight< Range4(0.0f, 0.0f, 0.0f, 0.0f, 16.0f, 16.0f, 16.0f, 16.0f); Default4(0.0f, 0.0f, 0.0f, 0.0f); >;
	float3 g_vTintColor< Range3(0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f); Default3(1.0f, 1.0f, 1.0f); >;
	int g_HueShift< Range(0, 64); Default(0); >;
	BoolAttribute( IsVoxelBlock, F_IS_VOXEL_BLOCK ? true : false );
	#endif
	
	float3 hueShift(float3 color, float hue) {
		const float3 k = float3(0.57735, 0.57735, 0.57735);
		float cosAngle = cos(hue);
		return float3(color * cosAngle + cross(k, color) * sin(hue) + k * dot(k, color) * (1.0 - cosAngle));
	}

	PixelOutput MainPs( PixelInput i )
	{
		float4 vColor = Tex2DLevelS( g_tColor, g_sPointSampler, i.vTextureCoords.xy, 0 );
	
		Material m = GatherMaterial( i );
		
		#if S_IS_VOXEL_BLOCK
			bool hasTintColor = (g_vTintColor.r < 1.0f || g_vTintColor.g < 1.0f || g_vTintColor.b < 1.0f );
			
			if ( g_HueShift > 0 || hasTintColor )
			{
				if ( hasTintColor )
					vColor.rgb *= g_vTintColor.rgb;
			
				if ( g_HueShift > 0 )
					vColor.rgb = hueShift( vColor.rgb, (3.14f / 128.0f) * g_HueShift );
			}
			
			float3 torchColor = (g_vVoxelLight.rgb / 16.0f) * (1.0f - (((sin(g_flTime * 8.0f) + 1.0f) * 0.5f) * 0.2f));
			float sunlight = g_vVoxelLight.w / 16.0f;
			sunlight *= sunlight;
			m.Albedo.rgb = vColor * saturate(0.007f + torchColor + sunlight);
		#endif
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}