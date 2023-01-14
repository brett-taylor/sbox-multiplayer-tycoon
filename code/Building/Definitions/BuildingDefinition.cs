using Sandbox;
using System.Collections.Generic;
using TycoonGame.Building.Placement;
using TycoonGame.Building.Restrictions;
using TycoonGame.World;

namespace TycoonGame.Building.Definitions;

[GameResource( "Building Definition", "building", "Placeable Building" )]
public partial class BuildingDefinition : GameResource
{
	public string Name { get; set; }

	public string Archetype { get; set; }

	public PlacementType PlacementType { get; set; }

	[ResourceType( "vmdl" )]
	public string ModelPath { get; set; }

	public WorldCoordinate Size => new WorldCoordinate( 1, 1 );

	public int Price { get; set; }

	public List<BuildingRestrictionType> Restrictions { get; set; } = new List<BuildingRestrictionType>();

	public WFCTileModel TileModel { get; set; }

	// TODO remove this when doing train lines and make a proper track system that both roads and tracks use
	public bool ProvideRoadConnection { get; set; }

	public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}
