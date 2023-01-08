using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using TycoonGame.Utilities;
using TycoonGame.Vehicles.Base;
using TycoonGame.Vehicles.Definitions;

namespace TycoonGame.Vehicles;

[Category( "Simulation" )]
public partial class VehicleManager : Entity
{
	private static readonly Logger LOGGER = LoggerUtils.CreateLogger( typeof( VehicleManager ) );

	[Net] public IList<VehicleGroup> VehicleGroups { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public bool BuyVehicleGroup(BaseVehicleDefinition vehicleDefinition, out VehicleGroup vehicleGroup )
	{
		if ( TycoonGame.Instance.CompanyManager.HasMoney( vehicleDefinition.Price ) )
		{
			TycoonGame.Instance.CompanyManager.RemoveMoney( vehicleDefinition.Price );
			vehicleGroup = CreateVehicleGroup( vehicleDefinition );
			return true;
		}
		else
		{
			vehicleGroup = null;
			return false;
		}
	}

	public VehicleGroup CreateVehicleGroup( BaseVehicleDefinition vehicleDefinition )
	{
		LOGGER.Info( $"Creating Vehicle Group with definition {vehicleDefinition.ResourcePath}" );

		var vehicleGroup = new VehicleGroup();
		vehicleGroup.Name = $"{vehicleDefinition.Name}";
		vehicleGroup.VehicleDefinition = vehicleDefinition;

		VehicleGroups.Add( vehicleGroup );

		return vehicleGroup;
	}

	public void DeployVehicleGroup( VehicleGroup vehicleGroup, Vector3 position, Rotation rotation )
	{
		LOGGER.Info( $"Deploying Vehicle Group {vehicleGroup.Name}" );

		var newVehicleEntity = TypeLibrary.Create<BaseVehicleEntity>( vehicleGroup.VehicleDefinition.EntityTypeName );
		newVehicleEntity.SetVehicleDefinition( vehicleGroup.VehicleDefinition );
		newVehicleEntity.Position = position;
		newVehicleEntity.Rotation = rotation;

		vehicleGroup.DeployedEntity = newVehicleEntity;
	}

	public void StoreVehicleGroup( VehicleGroup vehicleGroup )
	{
		if ( !vehicleGroup.IsDeployed() )
			return;

		LOGGER.Info( $"Storing Vehicle Group {vehicleGroup.Name}" );

		vehicleGroup.DeployedEntity.Delete();
		vehicleGroup.DeployedEntity = null;
	}

	[ConCmd.Admin( "create_vehicle" )]
	private static void ConCmd_CreateVehicle( string vehicleDefinitionPath )
	{
		if ( ConsoleSystem.Caller is null || ConsoleSystem.Caller.Pawn is not Player.Player player || !Game.IsServer )
		{
			return;
		}

		if ( ResourceLibrary.TryGet( vehicleDefinitionPath, out BaseVehicleDefinition vehicleDefinition ) ) 
		{
			var vehicleGroup = TycoonGame.Instance.VehicleManager.CreateVehicleGroup( vehicleDefinition );
			var position = new Vector3( player.InputHoveredWorldPosition, 200f );
			TycoonGame.Instance.VehicleManager.DeployVehicleGroup( vehicleGroup, position, Rotation.FromYaw(90f) ); 
		} 
		else
		{
			LOGGER.Warning( $"Attempted create vehicle command with invalid vehicle definition path: {vehicleDefinitionPath}" );
		}
	}
}
