using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using TycoonGame.Building.Definitions;
using TycoonGame.Utilities;
using TycoonGame.Vehicles;
using TycoonGame.Vehicles.Definitions;

namespace TycoonGame.Building.Archetypes;

[Category( "Buildings/Maintenance" )]
public partial class VehicleDepot : BaseBuilding
{
	private static readonly Logger LOGGER = LoggerUtils.CreateLogger( typeof( VehicleDepot ) );
	private static readonly string SPAWN_POINT_PREFIX = "spawnpoint";

	private static readonly string PROPERTY_VEHICLE_GROUP_TYPE_KEY = "VehicleGroupType";

	[Net]
	public IList<VehicleGroup> StoredVehicles { get; set; } 

	private WindowManager.Window Window { get; set; }
	private VehicleDepotStoreList VehicleDepotStoreList { get;set; }

	public override void SetBuildingDefinition( BuildingDefinition buildingDefinition )
	{
		base.SetBuildingDefinition( buildingDefinition );

		if (!buildingDefinition.Properties.ContainsKey(PROPERTY_VEHICLE_GROUP_TYPE_KEY))
		{
			LOGGER.Error( $"Vehicle Depot expects a { PROPERTY_VEHICLE_GROUP_TYPE_KEY } property" );
			return;
		}
		else
		{
			var value = buildingDefinition.Properties[PROPERTY_VEHICLE_GROUP_TYPE_KEY];
			if (!Enum.TryParse<VehicleGroupType>(value, true, out VehicleGroupType vehicleGroupType))
			{
				LOGGER.Error( $"Vehicle Depot expects the {PROPERTY_VEHICLE_GROUP_TYPE_KEY} property to be a valid VehicleGroupType enum" );
				return;
			}
		}
	}

	public override void Selected( Player.Player player )
	{
		base.Selected( player );

		if (Game.IsClient && Window == null)
		{
			Window = TycoonHud.Instance.WindowManager.ShowWindow( Name );
			Window.WindowFrame.OnClose += CloseWindow;

			VehicleDepotStoreList = new VehicleDepotStoreList();
			VehicleDepotStoreList.VehicleDepot = this;
			Window.WindowFrame.AddTab( "Store Vehicle", VehicleDepotStoreList );

			var vehicleDepotWithdrawList = new VehicleDepotWithdrawList();
			vehicleDepotWithdrawList.VehicleDepot = this;
			vehicleDepotWithdrawList.StoredVehicles = StoredVehicles;
			Window.WindowFrame.AddTab( "Withdraw Vehicle", vehicleDepotWithdrawList );

			var vehicleBuyList = new VehicleBuyList();
			vehicleBuyList.RoadDepot = this;
			vehicleBuyList.VehicleType = GetVehicleGroupType();
			Window.WindowFrame.AddTab( "New Vehicle", vehicleBuyList );
		}
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if (VehicleDepotStoreList != null)
		{
			VehicleDepotStoreList.StorableVehicles = TycoonGame.Instance.VehicleManager.VehicleGroups
				.Where( vg => vg.IsDeployed() )
				.Where( vg => GetVehicleGroupType() == vg.VehicleDefinition.Type )
				.ToList();
		}
	}

	private void CloseWindow()
	{
		Window = null;
		VehicleDepotStoreList = null;
	}

	private List<Transform> GetAllSpawnPoints()
	{
		return Enumerable
			.Range( 0, Model.AttachmentCount )
			.Where( i => Model.GetAttachmentName( i ).StartsWith( SPAWN_POINT_PREFIX ) )
			.Select( i => Model.GetAttachment( i ).Value )
			.ToList();
	}

	private VehicleGroupType GetVehicleGroupType()
	{
		return Enum.Parse<VehicleGroupType>( BuildingDefinition.Properties[PROPERTY_VEHICLE_GROUP_TYPE_KEY], true );
	}

	[ConCmd.Server]
	public static void Concmd_StoreVehicle( int roadDepotNetworkInt, int vehicleGroupNetworkIdent )
	{
		var roadDepot = FindByIndex<VehicleDepot>( roadDepotNetworkInt );
		var vehicleGroup = FindByIndex<VehicleGroup>( vehicleGroupNetworkIdent );

		if ( roadDepot != null && vehicleGroup != null )
		{
			TycoonGame.Instance.VehicleManager.StoreVehicleGroup( vehicleGroup );
			roadDepot.StoredVehicles.Add( vehicleGroup );
		}
	}

	[ConCmd.Server]
	public static void Concmd_WithdrawVehicle( int roadDepotNetworkInt, int vehicleGroupNetworkIdent )
	{
		var roadDepot = FindByIndex<VehicleDepot>( roadDepotNetworkInt );
		var vehicleGroup = FindByIndex<VehicleGroup>( vehicleGroupNetworkIdent );

		if ( roadDepot != null && vehicleGroup != null )
		{
			roadDepot.StoredVehicles.Remove( vehicleGroup );

			var allSpawnPoints = roadDepot.GetAllSpawnPoints();
			var selectedSpawnPoint = allSpawnPoints[new Random().Next( allSpawnPoints.Count )];
			var spawnPosition = roadDepot.Position + (selectedSpawnPoint.Position * roadDepot.Rotation );
			var spawnRotation = Rotation.FromYaw( roadDepot.Rotation.Yaw() - selectedSpawnPoint.Rotation.Yaw() );
			TycoonGame.Instance.VehicleManager.DeployVehicleGroup( vehicleGroup, spawnPosition, spawnRotation );
		}
	}

	[ConCmd.Server]
	public static void Concmd_BuyVehicle( int roadDepotNetworkInt, int vehicleDefinitionId )
	{
		var roadDepot = FindByIndex<VehicleDepot>( roadDepotNetworkInt );
		var vehicleDefinition = ResourceLibrary.Get<BaseVehicleDefinition>( vehicleDefinitionId );

		if ( roadDepot == null || vehicleDefinition == null )
		{
			return;
		}	
		
		if (TycoonGame.Instance.VehicleManager.BuyVehicleGroup(vehicleDefinition, out VehicleGroup newVehicleGroup))
		{
			roadDepot.StoredVehicles.Add( newVehicleGroup );
		}
		else
		{
			LOGGER.Warning( "[TODO UI Popups] No Money" );
		}
	}
}
