using Sandbox;
using Sandbox.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using TycoonGame.Building.Archetypes;
using TycoonGame.Building.Definitions;
using TycoonGame.Building.Restrictions;
using TycoonGame.Utilities;
using TycoonGame.World;
using TycoonGame.World.Data;

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
			var placeData = new PlaceData()
			{
				Position = hoveredWorldCell,
				Yaw = FixedSizePlacementGhostClient.Rotation.Yaw(),
			};

			BuildingController.ConCmd_PlaceBuilding( JsonSerializer.Serialize( placeData ) );
		}

		if (Input.Pressed(InputButton.SecondaryAttack ) ) 
		{
			BuildingController.ConCmd_StopBuilding();
		}

		LastHoveredWorldCell = hoveredWorldCell;
	}

	public override void PlaceBuilding( string data )
	{
		base.PlaceBuilding( data );

		var placeData = Json.Deserialize<PlaceData>( data );

		if ( !TycoonGame.Instance.CompanyManager.HasMoney( BuildingDefinition.Price ) )
		{
			LOGGER.Warning( "[TODO UI Popups] No Money" );
			return;
		}

		if (!BuildingRestrictionManager.Instance.IsValid( BuildingDefinition, placeData.Position, out BuildingRestriction buildingRestriction ))
		{
			LOGGER.Warning( $"[TODO UI Popups] Failed a restriction: { buildingRestriction.LastFailedReason }" );
			return;
		}

		TycoonGame.Instance.CompanyManager.RemoveMoney( BuildingDefinition.Price );

		var newBuildng = TypeLibrary.Create<BaseBuilding>( BuildingDefinition.Archetype );
		newBuildng.SetBuildingDefinition( BuildingDefinition );
		newBuildng.SetPosition( placeData.Position );
		newBuildng.Rotation = Rotation.FromYaw( placeData.Yaw );

		TycoonGame.Instance.WorldManager.RegisterNewBuilding( placeData.Position.WorldCoordinate, newBuildng );
	}

	private class PlaceData
	{
		public WorldCell Position { get; set; }
		public float Yaw { get; set; }
	}
}
