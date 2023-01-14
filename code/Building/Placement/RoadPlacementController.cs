using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using TycoonGame.Building.Archetypes;
using TycoonGame.Building.Definitions;
using TycoonGame.Building.Restrictions;
using TycoonGame.Utilities;
using TycoonGame.World;

namespace TycoonGame.Building.Placement;

public class RoadPlacementController : PlacementController
{
	private static readonly Logger LOGGER = LoggerUtils.CreateLogger( typeof( RoadPlacementController ) );

	private GridDisplay GridDisplayClient { get; set; }

	private WorldCell StartDragWorldCell { get; set; }
	private WorldCell EndDragWorldCell { get; set; }

	public RoadPlacementController( BuildingDefinition buildingDefinition ) : base( buildingDefinition )
	{
	}

	public override void StartBuildingClient()
	{
		base.StartBuildingClient();

		GridDisplayClient = new GridDisplay();
	}

	public override void StopBuildingClient()
	{
		base.StopBuildingClient();

		if ( GridDisplayClient != null )
		{
			GridDisplayClient.Delete();
			GridDisplayClient = null;
		}
	}

	public override void UpdateClient( IClient cl )
	{
		base.UpdateClient( cl );

		var hoveredWorldCell = (cl.Pawn as Player.Player).GetHoveredWorldCell();
		if ( hoveredWorldCell == null )
		{
			GridDisplayClient.EnableDrawing = false;
			return;
		}

		GridDisplayClient.SetPosition( hoveredWorldCell );
		GridDisplayClient.EnableDrawing = true;

		if ( StartDragWorldCell == null) 
		{
			UpdateClientNotDragging( hoveredWorldCell );
		}
		else
		{
			UpdateClientDragging( hoveredWorldCell );
		}
	}

	public override void PlaceBuilding( string data )
	{
		base.PlaceBuilding( data );

		DeserializePlacementString( data, out WorldCell startWorldCell, out WorldCell endWorldCell );

		if ( startWorldCell == null || endWorldCell == null )
		{
			return;
		}

		var buildPath = CalculateBuildPath( startWorldCell, endWorldCell );

		LOGGER.Info( "called" );
		if ( !PassMoneyCheck( buildPath, true ) || !PassBuildingRestrictionCheck( buildPath, true ) )
		{
			return;
		}

		TycoonGame.Instance.CompanyManager.RemoveMoney( CalculateTotalCost( buildPath ) );

		foreach( var worldCell in buildPath )
		{
			var newBuildng = TypeLibrary.Create<BaseBuilding>( BuildingDefinition.Archetype );
			newBuildng.SetBuildingDefinition( BuildingDefinition );
			newBuildng.SetPosition( worldCell );
		}
	}

	private void ResetDrag()
	{
		StartDragWorldCell = null;
		EndDragWorldCell = null;
	}

	private void UpdateClientNotDragging( WorldCell hoveredWorldCell )
	{
		if (Input.Pressed(InputButton.PrimaryAttack))
		{
			StartDragWorldCell = hoveredWorldCell;
		}

		if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{
			BuildingController.ConCmd_StopBuilding();
		}
	}

	private void UpdateClientDragging( WorldCell hoveredWorldCell )
	{
		if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{
			ResetDrag();
			return;
		}

		if (EndDragWorldCell != hoveredWorldCell)
		{
			EndDragWorldCell = hoveredWorldCell;
		}

		var buildPath = CalculateBuildPath( StartDragWorldCell, EndDragWorldCell );

		var isValid = PassBuildingRestrictionCheck( buildPath, false );

		buildPath.ForEach( path => DrawDebugSquare( path, isValid ? Color.Green : Color.Red, 0f ) );

		if ( Input.Pressed( InputButton.PrimaryAttack ) )
		{
			BuildingController.ConCmd_PlaceBuilding( SerializePlacementString( StartDragWorldCell, EndDragWorldCell ) );
			ResetDrag();
		}
	}

	private List<WorldCell> CalculateBuildPath( WorldCell start, WorldCell endCell )
	{
		var list = new List<WorldCell>();

		var minX = Math.Min( start.WorldCoordinate.X, endCell.WorldCoordinate.X );
		var minY = Math.Min( start.WorldCoordinate.Y, endCell.WorldCoordinate.Y );
		var maxX = Math.Max( start.WorldCoordinate.X, endCell.WorldCoordinate.X );
		var maxY = Math.Max( start.WorldCoordinate.Y, endCell.WorldCoordinate.Y );

		var xDistance = maxX - minX;
		var yDistance = maxY - minY;

		if (xDistance >= yDistance)
		{
			for ( var i = minX; i <= maxX; i++ )
			{
				list.Add( TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( i, start.WorldCoordinate.Y ) ) );
			}
		} 
		else
		{
			for ( var i = minY; i <= maxY; i++ )
			{
				list.Add( TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( start.WorldCoordinate.X, i ) ) );
			}
		}


		return list;
	}

	private void DrawDebugSquare( WorldCell worldCell, Color color, float duration )
	{
		var mins = worldCell.BottomLeftPosition() + new Vector3( 50, 50, 0 ) - new Vector3( 0, 0, 10 );
		var maxs = worldCell.TopRightPosition() - new Vector3( 50, 50, 0 ) + new Vector3( 0, 0, 100 );
		DebugOverlay.Box( mins, maxs, color, duration, true );
	}

	private bool PassBuildingRestrictionCheck( List<WorldCell> buildPath, bool showErrorPopup )
	{
		var isValid = true;
		BuildingRestriction failedBuildingRestriction = null;

		foreach ( var worldCell in buildPath )
		{
			if ( !BuildingRestrictionManager.Instance.IsValid( BuildingDefinition, worldCell, out BuildingRestriction buildingRestriction ) )
			{
				isValid = false;
				failedBuildingRestriction = buildingRestriction;
			}
		}

		if ( !isValid && showErrorPopup && failedBuildingRestriction != null )
		{
			LOGGER.Warning( $"[TODO UI Popups] Failed a restriction: {failedBuildingRestriction.LastFailedReason}" );
		}

		return isValid;
	}

	private int CalculateTotalCost( List<WorldCell> buildPath )
	{
		return BuildingDefinition.Price * buildPath.Count;
	}

	private bool PassMoneyCheck( List<WorldCell> buildPath, bool showErrorPopup ) 
	{
		if ( !TycoonGame.Instance.CompanyManager.HasMoney( CalculateTotalCost( buildPath ) ) )
		{
			if ( showErrorPopup )
			{
				LOGGER.Warning( "[TODO UI Popups] No Money" );
			}

			return false;
		}

		return true;
	}

	private string SerializePlacementString( WorldCell startWorldCell, WorldCell endWorldCell)
	{
		return $"{startWorldCell.WorldCoordinate.X},{startWorldCell.WorldCoordinate.Y},{endWorldCell.WorldCoordinate.X},{endWorldCell.WorldCoordinate.Y}";
	}


	private void DeserializePlacementString( string data, out WorldCell startWorldCell, out WorldCell endWorldCell)
	{
		startWorldCell = null;
		endWorldCell = null;

		var split = data.Split( ',' );

		if ( split.Length != 4 )
		{
			LOGGER.Error( $"Failed to deserialize placement string" );
			return;
		}

		var startWorldCellXIsNumeric = int.TryParse( split[0], out int startWorldCellX );
		var startWorldCellYIsNumeric = int.TryParse( split[1], out int startWorldCellY );
		var endWorldCellXIsNumeric = int.TryParse( split[2], out int endWorldCellX );
		var endWorldCellYIsNumeric = int.TryParse( split[3], out int endWorldCellY );

		if ( !startWorldCellXIsNumeric || !startWorldCellYIsNumeric || !endWorldCellXIsNumeric || !endWorldCellYIsNumeric )
		{
			LOGGER.Error( $"Failed to deserialize placement string" );
			return;
		}

		startWorldCell = TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( startWorldCellX, startWorldCellY ) );
		endWorldCell = TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( endWorldCellX, endWorldCellY ) );
	}
}
