using Sandbox;
using Sandbox.Diagnostics;
using TycoonGame.Building.Archetypes;
using TycoonGame.Building.Restrictions;
using TycoonGame.Utilities;
using TycoonGame.World;

namespace TycoonGame.Building.Placement;

public class FixedSizePlacementController : PlacementController
{
	private static readonly Logger LOGGER = LoggerUtils.CreateLogger( typeof( FixedSizePlacementController ) );

	private FixedSizePlacementGhost FixedSizePlacementGhostClient { get; set; }
	private GridDisplay GridDisplayClient { get; set; }

	private WorldCell LastHoveredWorldCell = null;
	private bool IsCurrentlyValid = false;

	public FixedSizePlacementController( BuildingDefinition buildingDefinition ) : base( buildingDefinition )
	{
	}

	public override void StartBuildingClient()
	{
		base.StartBuildingClient();

		FixedSizePlacementGhostClient = new FixedSizePlacementGhost();
		FixedSizePlacementGhostClient.SetBuildingDefinition( BuildingDefinition );
		FixedSizePlacementGhostClient.SetPosition( TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate(10, 10) ) );

		GridDisplayClient = new GridDisplay();
		GridDisplayClient.SetPosition( TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( 10, 10 ) ) );
	}

	public override void StopBuildingClient()
	{
		base.StopBuildingClient();

		if ( FixedSizePlacementGhostClient != null )
		{
			FixedSizePlacementGhostClient.Delete();
			FixedSizePlacementGhostClient = null;
		}

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
			FixedSizePlacementGhostClient.EnableDrawing = false;
			GridDisplayClient.EnableDrawing = false;
			return;
		}

		if (LastHoveredWorldCell != hoveredWorldCell)
		{
			IsCurrentlyValid = BuildingRestrictionManager.Instance.IsValid( BuildingDefinition, hoveredWorldCell, out BuildingRestriction ignored );
		}

		FixedSizePlacementGhostClient.SetPosition( hoveredWorldCell );
		FixedSizePlacementGhostClient.EnableDrawing = true;
		
		GridDisplayClient.SetPosition( hoveredWorldCell );
		GridDisplayClient.EnableDrawing = true;

		DebugOverlay.Box( FixedSizePlacementGhostClient.WorldSpaceBounds, IsCurrentlyValid ? Color.Green : Color.Red, 0f );

		if ( Input.Pressed( InputButton.Zoom ) || Input.Pressed( InputButton.Jump ) )
		{
			var newYaw = (FixedSizePlacementGhostClient.Rotation.Yaw() - 90f) % 360;
			FixedSizePlacementGhostClient.Rotation = Rotation.FromYaw( newYaw );
		}

		if ( Input.Pressed( InputButton.PrimaryAttack ) )
		{
			BuildingController.ConCmd_PlaceBuilding( SerializePlacementString( hoveredWorldCell, FixedSizePlacementGhostClient.Rotation.Yaw() ) );
		}

		LastHoveredWorldCell = hoveredWorldCell;
	}

	public override void PlaceBuilding( string data )
	{
		base.PlaceBuilding( data );

		DeserializePlacementString( data, out WorldCell worldCell, out float yaw );

		if ( worldCell == null )
		{
			return;
		}

		if ( !TycoonGame.Instance.CompanyManager.HasMoney( BuildingDefinition.Price ) )
		{
			LOGGER.Warning( "[TODO UI Popups] No Money" );
			return;
		}

		if (!BuildingRestrictionManager.Instance.IsValid( BuildingDefinition, worldCell, out BuildingRestriction buildingRestriction ))
		{
			LOGGER.Warning( $"[TODO UI Popups] Failed a restriction: { buildingRestriction.LastFailedReason }" );
			return;
		}

		TycoonGame.Instance.CompanyManager.RemoveMoney( BuildingDefinition.Price );

		var newBuildng = TypeLibrary.Create<BaseBuilding>( BuildingDefinition.Archetype );
		newBuildng.SetBuildingDefinition( BuildingDefinition );
		newBuildng.SetPosition( worldCell );
		newBuildng.Rotation = Rotation.FromYaw( yaw );
	}

	private string SerializePlacementString(WorldCell worldCell, float yaw)
	{
		return $"{worldCell.WorldCoordinate.X},{worldCell.WorldCoordinate.Y},{yaw}";
	}

	private void DeserializePlacementString( string data, out WorldCell worldCell, out float yaw )
	{
		worldCell = null;
		yaw = 0f;

		var split = data.Split( ',' );

		if (split.Length != 3)
		{
			LOGGER.Error( $"Failed to deserialize placement string" );
			return;
		}

		var worldCellXIsNumeric = int.TryParse( split[0], out int worldCellX );
		var worldCellYIsNumeric = int.TryParse( split[1], out int worldCellY );
		var yawIsNumeric = float.TryParse( split[2], out float rotationYaw );

		if ( !worldCellXIsNumeric || !worldCellYIsNumeric || !yawIsNumeric )
		{
			LOGGER.Error( $"Failed to deserialize placement string" );
			return;
		}

		worldCell = TycoonGame.Instance.WorldManager.GetWorldCell(new WorldCoordinate(worldCellX, worldCellY));
		yaw = rotationYaw;
	}
}
