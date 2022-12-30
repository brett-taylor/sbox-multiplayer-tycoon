using Sandbox;
using TycoonGame.Building.Core;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.World;

namespace TycoonGame.Building.Types;

public abstract class BaseBuilding : ModelEntity
{
	public BuildingDefinition BuildingDefinition { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		Tags.Add( CustomTags.Solid );
		Tags.Add( CustomTags.Building );
	}

	public virtual void SetBuildingDefinition(BuildingDefinition buildingDefinition)
	{
		BuildingDefinition = buildingDefinition;
		
		if (buildingDefinition.BuildingModelPath != null) {
			Name = BuildingDefinition.BuildingName;
			Model = Model.Load(buildingDefinition.BuildingModelPath);
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			EnableSelfCollisions = false;
			EnableShadowCasting = false;
			EnableTraceAndQueries = true;
		}
	}

	public void SetTilePosition( Vector2 tilePosition )
	{
		var offset = (BuildingDefinition.BuildingGridSize - Vector2.One) / 2;
		var offsetTilePosition = tilePosition - offset;

		Vector3 position = new Vector3(
			offsetTilePosition.x * WorldManager.TILE_SIZE_IN_UNITS.x + ( WorldManager.TILE_SIZE_IN_UNITS.x / 2),
			offsetTilePosition.y * WorldManager.TILE_SIZE_IN_UNITS.y + (WorldManager.TILE_SIZE_IN_UNITS.y / 2),
			1f
		);

		Position = position;
	}
}
