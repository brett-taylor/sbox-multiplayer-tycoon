using Sandbox;
using Sandbox.Diagnostics;
using TycoonGame.Building.Definitions;

namespace TycoonGame.Building.Placement;

public abstract class PlacementController
{
	protected BuildingDefinition BuildingDefinition { get; init; }

	public PlacementController( BuildingDefinition buildingDefinition )
	{
		BuildingDefinition = buildingDefinition;
	}

	public virtual void StartBuildingClient()
	{
		Assert.True( Game.IsClient );
	}

	public virtual void StopBuildingClient()
	{
		Assert.True( Game.IsClient );
	}

	public virtual void UpdateClient( IClient cl )
	{
		Assert.True( Game.IsClient );
	}

	public virtual void PlaceBuilding( string data )
	{
		Assert.True( Game.IsServer );
	}
}
