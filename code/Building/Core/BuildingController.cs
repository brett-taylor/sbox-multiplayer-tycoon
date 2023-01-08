using Sandbox;
using Sandbox.Diagnostics;
using System.Linq;
using TycoonGame.Building.Types;
using TycoonGame.Utilities;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.World;

namespace TycoonGame.Building.Core;

[Category( "Simulation" )]
public partial class BuildingController : Entity
{
	private static readonly Logger LOGGER = LoggerUtils.CreateLogger( typeof( BuildingController ) );

	[Net]
	private BuildingDefinition BuildingDefinition { get; set; }

	private BuildingGridDisplay BuildingGridDisplay { get; set; }

	private GhostBuilding GhostBuilding { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Owner;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		if ( !Game.IsClient )
			return;

		BuildingGridDisplay = new BuildingGridDisplay();
		BuildingGridDisplay.Owner = Owner;
		BuildingGridDisplay.EnableDrawing = false;

		GhostBuilding = new GhostBuilding();
		GhostBuilding.Owner = Owner;
		GhostBuilding.EnableDrawing = false;
	}

	public bool IsBuilding => BuildingDefinition != null;

	public void StartBuilding(BuildingDefinition buildingDefinition)
	{
		BuildingDefinition = buildingDefinition;
		StartBuildngClient( To.Single(Owner));
	}

	[ClientRpc]
	private void StartBuildngClient()
	{
		GhostBuilding.SetBuildingDefinition( BuildingDefinition );
		GhostBuilding.Rotation = Rotation.FromYaw( 0f );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( !IsBuilding || !Game.IsServer )
			return;

		if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{
			StopBuilding();
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( !IsBuilding )
		{
			BuildingGridDisplay.EnableDrawing = false;
			GhostBuilding.EnableDrawing = false;
			return;
		}

		using (Prediction.Off())
		{
			var hoveredWorldCell = (Owner as Player.Player).GetHoveredWorldCell();
			if ( hoveredWorldCell == null )
			{
				BuildingGridDisplay.EnableDrawing = false;
				GhostBuilding.EnableDrawing = false;
				return;
			}

			BuildingGridDisplay.SetPosition( hoveredWorldCell );
			BuildingGridDisplay.EnableDrawing = true;

			GhostBuilding.SetPosition( hoveredWorldCell );
			GhostBuilding.EnableDrawing = true;

			if ( Input.Pressed( InputButton.Zoom ) || Input.Pressed( InputButton.Jump ) )
			{
				var newYaw = (GhostBuilding.Rotation.Yaw() - 90f) % 360;
				GhostBuilding.Rotation = Rotation.FromYaw( newYaw );
			}

			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				ConCmd_PlaceBuilding( hoveredWorldCell.WorldCoordinate.X, hoveredWorldCell.WorldCoordinate.Y, GhostBuilding.Rotation );
			}
		}
	}

	private void StopBuilding()
	{
		BuildingDefinition = null;
	}

	private void PlaceBuilding(WorldCell worldCell, Rotation rotation)
	{
		if ( !TycoonGame.Instance.CompanyManager.HasMoney( BuildingDefinition.BuildingPrice ) )
		{
			LOGGER.Warning( "[TODO UI Popups] No Money" );
			return;
		}

		var deadzone = 50f;
		var bboxToCheck = new BBox( worldCell.BottomLeftPosition() + deadzone, worldCell.TopRightPosition() - deadzone );
		if ( FindInBox( bboxToCheck ).Where( entity => entity.Tags.Has( CustomTags.Building ) ).Any() )
		{
			LOGGER.Warning( "[TODO UI Popups] Building Collision" );
			return;
		}

		TycoonGame.Instance.CompanyManager.RemoveMoney( BuildingDefinition.BuildingPrice );

		var newBuildng = TypeLibrary.Create<BaseBuilding>( BuildingDefinition.BuildingArchetype );
		newBuildng.SetBuildingDefinition( BuildingDefinition );
		newBuildng.SetPosition( worldCell );
		newBuildng.Rotation = rotation;
	}

	[ConCmd.Server( "start_building" )]
	public static void ConCmd_StartBuilding(int buildDefinitionId)
	{
		if ( ConsoleSystem.Caller == null || ConsoleSystem.Caller.Pawn is not Player.Player player )
			return;

		if ( TycoonGame.Instance.CompanyManager.Ceo != player )
		{
			LOGGER.Warning( "[TODO UI Popups] You arent the CEO" );
			return;
		}

		player.BuildingController.StartBuilding( ResourceLibrary.Get<BuildingDefinition>( buildDefinitionId ) );
	}

	[ConCmd.Server( "place_building" )]
	public static void ConCmd_PlaceBuilding(int worldCoordinateX, int worldCoordinateY, Rotation rotation)
	{
		if ( ConsoleSystem.Caller == null || ConsoleSystem.Caller.Pawn is not Player.Player player )
			return;

		if (TycoonGame.Instance.CompanyManager.Ceo != player )
		{
			LOGGER.Warning( "[TODO UI Popups] You arent the CEO" );
			return;
		}

		var worldCell = TycoonGame.Instance.WorldManager.GetWorldCell( new WorldCoordinate( worldCoordinateX, worldCoordinateY ) );
		if (worldCell != null)
		{
			player.BuildingController.PlaceBuilding( worldCell, rotation );
		}
	}
}
