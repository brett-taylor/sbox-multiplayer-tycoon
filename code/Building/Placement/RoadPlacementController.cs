using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TycoonGame.Building.Archetypes;
using TycoonGame.Building.Definitions;
using TycoonGame.Building.Restrictions;
using TycoonGame.Utilities;
using TycoonGame.World;
using TycoonGame.World.Data;

namespace TycoonGame.Building.Placement;

public class RoadPlacementController : PlacementController
{
	private static readonly Logger LOGGER = LoggerUtils.CreateLogger( typeof( RoadPlacementController ) );

	private GridDisplay GridDisplayClient { get; set; }

	private WorldCell StartDragWorldCell { get; set; }
	private WorldCell EndDragWorldCell { get; set; }
	private WorldCell FirstTileHoveredAfterDragging { get; set; }
	private bool IsAnchored { get; set; }

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

		var placeData = Json.Deserialize<PlaceData>( data );
		StartDragWorldCell = placeData.StartDragWorldCell;
		EndDragWorldCell = placeData.EndDragWorldCell;
		FirstTileHoveredAfterDragging = placeData.FirstTileHoveredAfterDragging;
		IsAnchored = placeData.IsAnchored;

		var buildPath = CalculateBuildPath();

		if ( !PassMoneyCheck( buildPath, true ) || !PassBuildingRestrictionCheck( buildPath, true ) )
		{
			return;
		}

		TycoonGame.Instance.CompanyManager.RemoveMoney( CalculateTotalCost( buildPath ) );

		var worldCellsToPlaceOn = buildPath
			.Where( path => !TycoonGame.Instance.WorldManager.DoesBuildingTypeExistOn( path.WorldCoordinate, BuildingDefinition ) )
			.ToList();

		var newBuildings = new Dictionary<WorldCoordinate, BaseBuilding>();
		foreach( var worldCell in worldCellsToPlaceOn )
		{
			var newBuildng = TypeLibrary.Create<BaseBuilding>( BuildingDefinition.Archetype );
			newBuildng.SetBuildingDefinition( BuildingDefinition );
			newBuildng.SetPosition( worldCell );
			
			newBuildings.Add( worldCell.WorldCoordinate, newBuildng );
		}

		TycoonGame.Instance.WorldManager.RegisterNewBuildings( newBuildings );
	}

	private void ResetDrag()
	{
		StartDragWorldCell = null;
		EndDragWorldCell = null;
		IsAnchored = false;
		FirstTileHoveredAfterDragging = null;
	}

	private void UpdateClientNotDragging( WorldCell hoveredWorldCell )
	{
		if (Input.Pressed(InputButton.PrimaryAttack))
		{
			StartDragWorldCell = hoveredWorldCell;
			IsAnchored = TycoonGame.Instance.WorldManager.DoesBuildingTypeExistOn( hoveredWorldCell.WorldCoordinate, BuildingDefinition );
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

		if (FirstTileHoveredAfterDragging != null && StartDragWorldCell == hoveredWorldCell)
		{
			FirstTileHoveredAfterDragging = null;
		}

		if ( FirstTileHoveredAfterDragging == null && StartDragWorldCell != hoveredWorldCell)
		{
			FirstTileHoveredAfterDragging = hoveredWorldCell;
		}

		if ( EndDragWorldCell != hoveredWorldCell )
		{
			EndDragWorldCell = hoveredWorldCell;
		}

		var buildPath = CalculateBuildPath();
		var isValid = PassBuildingRestrictionCheck( buildPath, false );
		foreach(var path in buildPath)
		{
			var sameBuildDef = TycoonGame.Instance.WorldManager.DoesBuildingTypeExistOn( path.WorldCoordinate, BuildingDefinition );
			DrawDebugSquare( path, !isValid ? Color.Red : sameBuildDef ? Color.Orange : Color.Green, 0f );
		}

		if ( Input.Pressed( InputButton.PrimaryAttack ) )
		{
			var placeData = new PlaceData
			{
				StartDragWorldCell = StartDragWorldCell,
				EndDragWorldCell = EndDragWorldCell,
				FirstTileHoveredAfterDragging = FirstTileHoveredAfterDragging,
				IsAnchored = IsAnchored
			};

			BuildingController.ConCmd_PlaceBuilding( JsonSerializer.Serialize( placeData ) );

			ResetDrag();
		}
	}

	private List<WorldCell> CalculateBuildPath()
	{
		var list = new List<WorldCell>();

		var minX = Math.Min( StartDragWorldCell.WorldCoordinate.X, EndDragWorldCell.WorldCoordinate.X );
		var minY = Math.Min( StartDragWorldCell.WorldCoordinate.Y, EndDragWorldCell.WorldCoordinate.Y );
		var maxX = Math.Max( StartDragWorldCell.WorldCoordinate.X, EndDragWorldCell.WorldCoordinate.X );
		var maxY = Math.Max( StartDragWorldCell.WorldCoordinate.Y, EndDragWorldCell.WorldCoordinate.Y );

		var xDistance = maxX - minX;
		var yDistance = maxY - minY;

		if ( IsAnchored && FirstTileHoveredAfterDragging != null)
		{
			// Follow the mouse by always going to the first tile hovered after starting the drag.
			if ( Math.Abs( FirstTileHoveredAfterDragging.WorldCoordinate.X - StartDragWorldCell.WorldCoordinate.X ) >= 1)
			{
				// In this case they moved x first so do x first
				for ( var i = minX; i <= maxX; i++ )
				{
					list.Add( TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( i, StartDragWorldCell.WorldCoordinate.Y ) ) );
				}

				for ( var i = minY; i <= maxY; i++ )
				{
					var wc = TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( EndDragWorldCell.WorldCoordinate.X, i ) );
					if ( !list.Contains( wc ) )
					{
						list.Add( wc );
					}
				}
			} 
			else
			{
				// In this case they moved y first so do y first
				for ( var i = minY; i <= maxY; i++ )
				{
					list.Add( TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( StartDragWorldCell.WorldCoordinate.X, i ) ) );
				}

				for ( var i = minX; i <= maxX; i++ )
				{
					var wc = TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( i, EndDragWorldCell.WorldCoordinate.Y ) );
					if (!list.Contains( wc ) )
					{
						list.Add( wc );
					}
				}
			}
		}
		else if ( IsAnchored )
		{
			list.Add( StartDragWorldCell );
		}
		else
		{
			if ( xDistance >= yDistance )
			{
				for ( var i = minX; i <= maxX; i++ )
				{
					list.Add( TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( i, StartDragWorldCell.WorldCoordinate.Y ) ) );
				}
			}
			else
			{
				for ( var i = minY; i <= maxY; i++ )
				{
					list.Add( TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( StartDragWorldCell.WorldCoordinate.X, i ) ) );
				}
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
		var filteredCount = buildPath
			.Where( path => !TycoonGame.Instance.WorldManager.DoesBuildingTypeExistOn( path.WorldCoordinate, BuildingDefinition ) )
			.Count();

		return BuildingDefinition.Price * filteredCount;
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

	private class PlaceData
	{
		public WorldCell StartDragWorldCell { get; set; }
		public WorldCell EndDragWorldCell { get; set; }
		public WorldCell FirstTileHoveredAfterDragging { get; set; }
		public bool IsAnchored { get; set; }
	}
}
