using Sandbox;
using System.Collections.Generic;
using TycoonGame.Building.Placement;
using TycoonGame.Building.Restrictions;
using TycoonGame.World;

namespace TycoonGame.Building.Definitions;

[GameResource( "Building Definition", "building", "Placeable building" )]
public partial class BuildingDefinition : GameResource
{
	public string Name { get; set; }

	public string Archetype { get; set; }

	public PlacementType PlacementType { get; set; }

	[ResourceType( "vmdl" )]
	public string ModelPath { get; set; }

	public WorldCoordinate Size => new WorldCoordinate( 1, 1 );

	public int Price { get; set; }

	public List<BuildingRestrictionType> Restrictions { get; set; }

	public Dictionary<string, string> Properties { get; set; }
}
