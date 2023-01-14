using Sandbox;

namespace TycoonGame.Building.Definitions;

[GameResource( "WFC Tile Model Definition", "wfctm", "WFC Tile Model" )]
public class WFCTileModel : GameResource
{
	[ResourceType( "vmdl" )]
	public string NoConnectionsModel { get; set; }

	[ResourceType( "vmdl" )]
	public string OneConnectionsModel { get; set; }

	[ResourceType( "vmdl" )]
	public string TwoStraightConnectionsModel { get; set; }

	[ResourceType( "vmdl" )]
	public string TwoTurnConnectiosnModel { get; set; }

	[ResourceType( "vmdl" )]
	public string ThreeConnectionsModel { get; set; }

	[ResourceType( "vmdl" )]
	public string FourConnectionsModel { get; set; }
}
