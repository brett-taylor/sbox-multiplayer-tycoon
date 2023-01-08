using Sandbox;
using Sandbox.Diagnostics;
using TycoonGame.Building.Placement;
using TycoonGame.Utilities;

namespace TycoonGame.Building;

[Category( "Simulation" )]
public partial class BuildingController : Entity
{
	private static readonly Logger LOGGER = LoggerUtils.CreateLogger( typeof( BuildingController ) );

	[Net, Change]
	private BuildingDefinition BuildingDefinition { get; set; }

	private PlacementController PlacementController { get; set; }

	public bool IsBuilding()
	{
		return BuildingDefinition != null;
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Owner;
	}

	public void StartBuilding( BuildingDefinition buildingDefinition )
	{
		Assert.True( Game.IsServer );

		BuildingDefinition = buildingDefinition;
	}

	public void StopBuilding()
	{
		Assert.True( Game.IsServer );

		BuildingDefinition = null;
	}

	public void OnBuildingDefinitionChanged( BuildingDefinition oldValue, BuildingDefinition newValue )
	{
		Assert.True( Game.IsClient );

		if (newValue == null)
		{
			PlacementController.StopBuildingClient();
			PlacementController = null;
		} 
		else
		{
			if (PlacementController != null)
			{
				PlacementController.StopBuildingClient();
			}

			PlacementController = CreateBuildingPlacement( newValue );
			PlacementController.StartBuildingClient();
		}
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( !IsBuilding() || !Game.IsServer )
			return;

		if ( Input.Pressed( InputButton.SecondaryAttack ) )
			StopBuilding();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		if ( !IsBuilding() )
		{
			return;
		}

		using ( Prediction.Off() )
		{
			PlacementController.UpdateClient( cl );
		}
	}

	private PlacementController CreateBuildingPlacement( BuildingDefinition buildingDefinition )
	{
		var placementController = buildingDefinition.PlacementType switch
		{
			BuildingPlacementType.FIXED_SIZE => new FixedSizePlacementController( buildingDefinition ),
			_ => throw new System.Exception( $"{buildingDefinition.ResourcePath} does not have a placement type set." )
		};

		return placementController;
	}

	private void PlaceBuilding( string data )
	{
		var placementController = CreateBuildingPlacement( BuildingDefinition );
		placementController.PlaceBuilding( data );
	}

	[ConCmd.Server( "start_building" )]
	public static void ConCmd_StartBuilding( int buildDefinitionId )
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
	public static void ConCmd_PlaceBuilding( string data )
	{
		if ( ConsoleSystem.Caller == null || ConsoleSystem.Caller.Pawn is not Player.Player player )
			return;

		if ( TycoonGame.Instance.CompanyManager.Ceo != player )
		{
			LOGGER.Warning( "[TODO UI Popups] You arent the CEO" );
			return;
		}

		if (!player.BuildingController.IsBuilding())
		{
			LOGGER.Warning( $"ConCmd_PlaceBuilding attempted to be called by player {player.Client.Name}, who isn't building." );
			return;
		}

		player.BuildingController.PlaceBuilding( data );
	}
}
