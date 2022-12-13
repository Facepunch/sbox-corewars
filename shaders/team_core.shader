//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "Template Shader for S&box";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
MODES
{
    VrForward();													// Indicates this shader will be used for main rendering
    Depth( "vr_depth_only.vfx" ); 									// Shader that will be used for shadowing and depth prepass
    ToolsVis( S_MODE_TOOLS_VIS ); 									// Ability to see in the editor
    ToolsWireframe( "vr_tools_wireframe.vfx" ); 					// Allows for mat_wireframe to work
	ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" ); 	// Shows how expensive drawing is in debug view
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
    nointerpolation uint nIsOutline : TEXCOORD14;
    nointerpolation float flCurrentOutline : TEXCOORD15;
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"

    float g_flMinThickness< Default( 0.2f ); Range(0.0f, 0.5f); UiGroup( "Core,20/Outline,10/1" ); >;
    float g_flMaxThickness< Default( 3.0f ); Range(0.0f, 10.0f); UiGroup( "Core,20/Outline,10/2" ); >;

	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
        uint nView = uint( 0 );
        uint nSubview = uint( 0 );
        #if ( D_MULTIVIEW_INSTANCING )
            GetViewAndSubview( i.nInstanceID, nView, nSubview );
        #endif
        float3x4 matObjectToWorld = CalculateInstancingObjectToWorldMatrix( INSTANCING_PARAMS( i ) );

		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
        o.nIsOutline = 0;

        float3 vBasePosition = mul( matObjectToWorld, float4(float3( 0, 0, 0 ), 1.0 ) );
        const float flCameraLeeway = 64.0f;
        float flCamDistance = max( 0.0f, distance( vBasePosition, g_vCameraPositionWs ) - flCameraLeeway );
        flCamDistance = saturate( lerp(0.0f, 1.0f, flCamDistance / 100.0f ) );
        float flOutlineSize = lerp(g_flMinThickness, g_flMaxThickness, flCamDistance);

        o.flCurrentOutline = flOutlineSize;

		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

GS
{
    [maxvertexcount(6)]
    void MainGs(triangle in PixelInput vertices[3], inout TriangleStream<PixelInput> triStream)
    {
        int i = 0;

        PixelInput v[3];
        for ( i = 0; i < 3; i++ )
        {
            triStream.Append(vertices[i]);
            v[i] = vertices[i];
        }
        
        // emit the flipped vertices
        triStream.RestartStrip();
        
        // color all vertices black
        // Convert these to world space
        for ( i = 0; i < 3; i++ )
        {
            v[i].vPositionWs = v[i].vPositionWs + ( v[i].vNormalWs * v[i].flCurrentOutline );
            v[i].vPositionWs += g_vHighPrecisionLightingOffsetWs.xyz;
            v[i].vPositionPs = Position3WsToPsMultiview( 0, v[i].vPositionWs ) + float4(0,0,1,0);
            v[i].nIsOutline = 1;
        }

        triStream.Append(v[2]);
        triStream.Append(v[0]);
        triStream.Append(v[1]);
    }
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"
    SamplerState g_sBilinearWrap < Filter( BILINEAR ); AddressU( WRAP ); AddressV( WRAP ); >;
    
    float g_flExponent< Default( 2.0f ); Range(0.0f, 16.0f); UiGroup( "Core,10/Fresnel,10/1" ); >;
    float g_flReflectance< Default( 0.0f ); Range(0.0f, 2.0f); UiGroup( "Core,10/Fresnel,10/2" ); >;
    float g_flRefractionScale< Default(1.0f); Range(0.0f, 8.0f); UiGroup( "Core,10/Fresnel,10/3" ); >;

    float g_flRainbowSpeed< Default( 4.0f ); Range(0.0f, 32.0f); UiGroup( "Core,30/Rainbow,10/1" ); >;
    float g_flRainbowBandMultiplier< Default( 1.0f ); Range(0.0f, 32.0f); UiGroup( "Core,30/Rainbow,10/2" ); >;
    float g_flRainbowColorMultiplier< Default( 1.0f ); Range(0.0f, 32.0f); UiGroup( "Core,30/Rainbow,10/3" ); >;
    float g_flCurveAmount< Default(0.503f); Range(0.0f, 1.0f); UiGroup( "Core,30/Rainbow,10/4" ); >;

	CreateInputTextureCube( TextureInteriorMap, Linear, 8, "", "_cube", "Material,10/30", Default3( 0.5, 0.5, 0.5 ) );
	CreateTextureCube( g_tCubeMap ) < Channel( RGB, Box( TextureInteriorMap ), Linear ); OutputFormat( RGBA16161616F ); SrgbRead( false ); >;
	DECLARE_TEXTURE_DIM_VAR( g_tCubeMap );

    CreateInputTexture2D( TextureCurve, Linear, 8, "", "_curve",  "Material,10/40", Default( 0.5 ) );
    CreateTexture2DWithoutSampler( g_tCurve ) < Channel( R, Box( TextureCurve ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;

	//
	// Main
	//
	float4 MainPs( PixelInput i ) : SV_Target0
	{
        float4 vColor = float4(0,0,0,1);

        float3 PositionWithOffsetWs = i.vPositionWithOffsetWs.xyz;
        float3 PositionWs = PositionWithOffsetWs + g_vCameraPositionWs;

        float flCurve = Tex2DLevelS( g_tCurve, g_sBilinearWrap, i.vTextureCoords, 0 ).r * 2.0f - 1.0f;

        const float3 vRayOrigin = g_vCameraPositionWs;
        const float3 vRayDirection = CalculatePositionToCameraDirWs( g_vCameraPositionWs - i.vPositionWithOffsetWs ); 

        float fRefractionFresnel = CalculateNormalizedFresnel( g_flReflectance, g_flExponent, PositionWs, normalize( i.vNormalWs.xyz ) );
        float3 vTexColor = TexCubeLevel( g_tCubeMap, normalize(vRayDirection + (i.vNormalWs * (flCurve * g_flRefractionScale))), 0 ).rgb;
        vColor.rgb = (fRefractionFresnel * i.vVertexColor.rgb) + ((1.0f - fRefractionFresnel) * vTexColor);

        if( i.nIsOutline)
        {
            float3 UP = float3(0,0,-1);
            float zUpAmount = dot(PositionWithOffsetWs, UP) + (g_flTime * g_flRainbowSpeed);
            float3 vHsv = float3(frac((zUpAmount * 0.02f) * g_flRainbowBandMultiplier), 1.0f, 1.0f);
            float3 vRainbow = HsvToRgb( vHsv ).rgb;
            vColor.rgb = vRainbow.rgb * g_flRainbowColorMultiplier;
        }

        return vColor;
	}
}