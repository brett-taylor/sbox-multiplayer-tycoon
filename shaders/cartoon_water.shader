//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	Description = "Cartoon Water";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"

	float4 _DepthGradientShallow < Default4( 0.325, 0.807, 0.971, 0.725 ); UiType( Color ); UiGroup( "Cartoon Water" ); >;
	float4 _DepthGradientDeep < Default4( 0.086, 0.407, 1, 0.749 ); UiType( Color ); UiGroup( "Cartoon Water" ); >;
	float _DepthMaxDistance < Default( 1 ); UiGroup( "Cartoon Water" ); >;

	CreateTexture2D( _ColorBuffer ) < Attribute( "ColorBuffer" ); SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
	CreateTexture2D( _DepthBuffer ) < Attribute( "DepthBuffer" ); SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
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
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"
	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );
		o.vPositionWs.z += sin(g_flTime) * 500.0f;
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = GatherMaterial( i );
		return _DepthGradientDeep;

		// float2 screenUv = CalculateViewportUv( i.vPositionSs.xy ); 
		// float4 c = Tex2D( _ColorBuffer, screenUv );
		// return c;

		// float2 screenUv = CalculateViewportUv( i.vPositionWithOffsetWs.xy ); 
		// float worldDepth = Tex2D( _ColorBuffer, screenUv ).r;
        // worldDepth = RemapValClamped( worldDepth, g_flViewportMinZ, g_flViewportMaxZ, 0.0, 1.0 );
		// return worldDepth;

		// float2 vPositionUv = ScreenspaceCorrectionMultiview( CalculateViewportUvFromInvSize( i.vPositionSs.xy - g_vViewportOffset.xy, g_vInvViewportSize.xy ) );
		// float worldDepth = Tex2D( _ColorBuffer, vPositionUv ).r;
		// worldDepth = RemapValClamped( worldDepth, g_flViewportMinZ, g_flViewportMaxZ, 0.0, 1.0 );
        // return float4(worldDepth, worldDepth, worldDepth, 1);

        // float4 a = Tex2D( _ColorBuffer, i.vTextureCoords.xy ).r;
		// a = RemapValClamped( a, g_flViewportMinZ, g_flViewportMaxZ, 0.0, 1.0 );
		// return a;
	}
}
