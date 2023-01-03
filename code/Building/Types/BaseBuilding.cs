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

	public void SetPosition( WorldCell worldCell )
	{
		Position = worldCell.CenterTilePosition();
	}
}
