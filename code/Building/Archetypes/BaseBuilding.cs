using Sandbox;
using Sandbox.Component;
using TycoonGame.Building.Definitions;
using TycoonGame.Player;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.World.Data;

namespace TycoonGame.Building.Archetypes;

public abstract partial class BaseBuilding : ModelEntity, IInteractableEntity
{
	private static readonly Color GLOW_COLOR = new Color32( 150, 182, 216 );

	[Net]
	public BuildingDefinition BuildingDefinition { get; private set; }

	public virtual bool IsSelectable => true;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		Tags.Add( CustomTags.Solid );
		Tags.Add( CustomTags.Building );
	}

	public virtual void SetBuildingDefinition(BuildingDefinition buildingDefinition)
	{		
		if (buildingDefinition.ModelPath != null) {
			Name = buildingDefinition.Name;
			Model = Model.Load(buildingDefinition.ModelPath);
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

			EnableSelfCollisions = false;
			EnableShadowCasting = false;
			EnableTraceAndQueries = true;
		}

		BuildingDefinition = buildingDefinition;
	}

	public void SetPosition( WorldCell worldCell )
	{
		Position = worldCell.CenterTilePosition();
	}

	public virtual bool CanSelect( Player.Player player )
	{
		return true;
	}

	public virtual void Selected( Player.Player player )
	{
	}

	public virtual void Unselected( Player.Player player )
	{
	}

	public virtual void Hovered( Player.Player player )
	{
		var glow = Components.GetOrCreate<Glow>();
		glow.Enabled = true;
		glow.Width = 0.5f;
		glow.Color = GLOW_COLOR;
	}

	public virtual void Unhovered( Player.Player player )
	{
		Components.GetOrCreate<Glow>().Enabled = false;
	}
}
