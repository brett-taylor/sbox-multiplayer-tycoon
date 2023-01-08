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
// Optional
//=========================================================================================================================
MODES
{
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"

	float4 _DepthGradientShallow < Default4( 0.325, 0.807, 0.971, 0.725 ); UiType( Color ); UiGroup( "Cartoon Water" ); >;
	float4 _DepthGradientDeep < Default4( 0.086, 0.407, 1, 0.749 ); UiType( Color ); UiGroup( "Cartoon Water" ); >;
	float _DepthMaxDistance < Default( 1 ); UiGroup( "Cartoon Water" ); >;

	CreateTexture2D( _DepthBuffer ) < Attribute( "DepthBuffer" ); SrgbRead( true ); Filter( POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >;
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
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================

PS
{
	// RenderState( BlendEnable, true );
	// RenderState( SrcBlend, SRC_ALPHA );
	// RenderState( DstBlend, ONE ); // RenderState( DstBlend, F_ADDITIVE_BLEND ? ONE : INV_SRC_ALPHA );

    // RenderState( DepthWriteEnable, true );
    // RenderState( DepthEnable, false );

	// #define BLEND_MODE_ALREADY_SET
	// #define DEPTH_STATE_ALREADY_SET

    #include "common/pixel.hlsl"

	float3 RebuildWorldPosition( float flProjectedDepth, float3 vCameraRayWs )
	{
	    float flZScale = g_vInvProjRow3.z;
	    float flZTran = g_vInvProjRow3.w;
    
	    // Get our actual depth
	    float flDepthRayLength = flProjectedDepth * flZScale + flZTran;
    
	    // Get the length relative to our camera direction
	    float flRelativeRayLength = flDepthRayLength * dot( g_vCameraDirWs.xyz, vCameraRayWs.xyz );
	    
		// Reconstruct the world position
    	return g_vCameraPositionWs.xyz + ( vCameraRayWs.xyz / flRelativeRayLength );
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m = GatherMaterial( i );
		float waterHeight = 10;
		
		// Get the current screen texture coordinates
    	float2 vScreenUv = i.vPositionSs.xy / g_vRenderTargetSize;

		// Get our depth and make it linear
    	float flDepth = Tex2D( _DepthBuffer, vScreenUv.xy ).r;
		flDepth = RemapValClamped( flDepth, g_flViewportMinZ, g_flViewportMaxZ, 0.0, 1.0 );

		// Construct our camera ray in world space
    	float2 vProjectedCoords = ( vScreenUv * 2.0f - 1.0f ) * float2(1.0f, -1.0f);
    	float3 vCameraRayWs = normalize( mul( g_matProjectionToWorld, float4( vProjectedCoords, 1.0f, 1 ) ).xyz );

		// Get our world position
    	float3 vWorldPosition = RebuildWorldPosition( flDepth, vCameraRayWs.xyz );

		float depthDifference = waterHeight - vWorldPosition.z;

		float waterDepthDifference01 = saturate( depthDifference / _DepthMaxDistance );
		float4 waterColor = lerp( _DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01 );
		return waterColor;
	}
}
