using Sandbox;
using Sandbox.Component;
using System;
using TycoonGame.Building.Definitions;
using TycoonGame.Player;
using TycoonGame.Utilities;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.World;
using TycoonGame.World.Data;

namespace TycoonGame.Building.Archetypes;

public abstract partial class BaseBuilding : ModelEntity, IInteractableEntity
{
	private static readonly Color GLOW_COLOR = new Color32( 150, 182, 216 );

	[Net, Predicted]
	public BuildingDefinition BuildingDefinition { get; private set; }

	[Net, Predicted]
	public WorldCoordinate WorldCoordinate { get; private set; }

	public virtual bool IsSelectable => true;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		Tags.Add( CustomTags.Solid );
		Tags.Add( CustomTags.Building );
	}

	public virtual void SetBuildingDefinition( BuildingDefinition buildingDefinition )
	{
		BuildingDefinition = buildingDefinition;

		EnableSelfCollisions = false;
		EnableShadowCasting = false;
		EnableTraceAndQueries = true;

		Name = BuildingDefinition.Name;

		if ( WorldCoordinate != null )
		{
			UpdateModel();
		}
	}

	public void SetPosition( WorldCell worldCell )
	{
		WorldCoordinate = worldCell.WorldCoordinate;
		Position = worldCell.CenterTilePosition();

		if ( BuildingDefinition != null )
		{
			UpdateModel();
		}
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

	public void NeighbourUpdated( WorldCell neighbour )
	{
		var doesSelfProvideRoadConnection = BuildingDefinition.ProvideRoadConnection;
		var isOtherBuilding = TycoonGame.Instance.WorldManager.DoesBuildingExistOn( neighbour.WorldCoordinate, out BaseBuilding buildingTwo );

		if ( doesSelfProvideRoadConnection && isOtherBuilding && buildingTwo.BuildingDefinition.ProvideRoadConnection )
		{
			UpdateModel();
		}
	}

	private void UpdateModel()
	{
		if ( BuildingDefinition.TileModel == null )
		{
			Model = Model.Load( BuildingDefinition.ModelPath );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		}
		else
		{
			// TODO Redo at some point

			var worldCell = TycoonGame.Instance.WorldManager.GetWorldCell( WorldCoordinate );
			var north = DoesWorldCellProvideRoadConnection( worldCell.North() );
			var east = DoesWorldCellProvideRoadConnection( worldCell.East() );
			var south = DoesWorldCellProvideRoadConnection( worldCell.South() );
			var west = DoesWorldCellProvideRoadConnection( worldCell.West() );
			
			var totalNeighbors = 0;
			totalNeighbors += north ? 1 : 0;
			totalNeighbors += east ? 1 : 0;
			totalNeighbors += south ? 1 : 0;
			totalNeighbors += west ? 1 : 0;

			if ( totalNeighbors == 0 )
			{
				Model = Model.Load( BuildingDefinition.TileModel.NoConnectionsModel );
			}

			if ( totalNeighbors == 1 )
			{
				Model = Model.Load( BuildingDefinition.TileModel.OneConnectionsModel);

				if ( north || south )
				{
					Rotation = Rotation.FromYaw( 0f );
				}

				if ( east || west )
				{
					Rotation = Rotation.FromYaw( 90f );
				}
			}

			if ( totalNeighbors == 2 )
			{
				if ( north && south)
				{
					Model = Model.Load( BuildingDefinition.TileModel.TwoStraightConnectionsModel );
				}

				if ( east && west )
				{
					Model = Model.Load( BuildingDefinition.TileModel.TwoStraightConnectionsModel );
					Rotation = Rotation.FromYaw( 90f );
				}

				if ( north && east )
				{
					Model = Model.Load( BuildingDefinition.TileModel.TwoTurnConnectiosnModel );
					Rotation = Rotation.FromYaw( -90f );
				}

				if ( east && south )
				{
					Model = Model.Load( BuildingDefinition.TileModel.TwoTurnConnectiosnModel );
					Rotation = Rotation.FromYaw( 180f );
				}

				if ( south && west )
				{
					Model = Model.Load( BuildingDefinition.TileModel.TwoTurnConnectiosnModel );
					Rotation = Rotation.FromYaw( 90f );
				}

				if ( north && west )
				{
					Model = Model.Load( BuildingDefinition.TileModel.TwoTurnConnectiosnModel );
					Rotation = Rotation.FromYaw( 0f );
				}
			}

			if ( totalNeighbors == 3 )
			{
				Model = Model.Load( BuildingDefinition.TileModel.ThreeConnectionsModel );

				if ( north && west && south )
				{
					Rotation = Rotation.FromYaw( 90f );
				}

				if ( west && south && east )
				{
					Rotation = Rotation.FromYaw( 180f );
				}

				if ( south && east && north )
				{
					Rotation = Rotation.FromYaw( -90f );
				}

				if ( north && east && west )
				{
					Rotation = Rotation.FromYaw( 0f );
				}
			}

			if ( totalNeighbors == 4 )
			{
				Model = Model.Load( BuildingDefinition.TileModel.FourConnectionsModel );
			}

			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		}
	}

	private bool DoesWorldCellProvideRoadConnection( WorldCell worldCell )
	{
		if ( worldCell == null )
			return false;

		if ( TycoonGame.Instance.WorldManager.DoesBuildingExistOn( worldCell.WorldCoordinate, out BaseBuilding buildingOther ) )
		{
			return buildingOther.BuildingDefinition.ProvideRoadConnection;
		}

		return false;
	}
}
