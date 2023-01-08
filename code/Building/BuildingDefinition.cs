using Sandbox;
using TycoonGame.World;

namespace TycoonGame.Building;

[GameResource( "Building Definition", "building", "Placeable building" )]
public partial class BuildingDefinition : GameResource
{
	public string Name { get; set; }

	public string Archetype { get; set; }

	public BuildingPlacementType PlacementType { get; set; }

	[ResourceType( "vmdl" )]
	public string ModelPath { get; set; }

	public WorldCoordinate Size => new WorldCoordinate( 1, 1 );

	public int Price { get; set; }
}
