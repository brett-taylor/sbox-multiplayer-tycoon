using Sandbox;

namespace TycoonGame.Building.Core;

[GameResource( "Building Definition", "building", "Placeable building" )]
public partial class BuildingDefinition : GameResource
{
	public string BuildingName { get; set; }

	public string BuildingArchetype { get; set; }

	[ResourceType( "vmdl" )]
	public string BuildingModelPath { get; set; }

	public Vector2 BuildingGridSize { get; set; }

	public int BuildingPrice { get; set; }
}
