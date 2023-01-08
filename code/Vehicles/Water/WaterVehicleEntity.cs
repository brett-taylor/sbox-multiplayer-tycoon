using Sandbox;
using Sandbox.Diagnostics;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.Vehicles.Base;
using TycoonGame.Vehicles.Definitions;

namespace TycoonGame.Vehicles.Water;

[Category( "Vehicles/WaterVehicles" )]
public partial class WaterVehicleEntity : BaseVehicleEntity
{
	private static readonly Logger LOGGER = new Logger( typeof( WaterVehicleEntity ).Name );

	public WaterVehicleDefinition WaterVehicleDefinition => VehicleDefinition as WaterVehicleDefinition;

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( CustomTags.WaterVehicle );
	}

	[Event.Physics.PreStep]
	public void PhysicsPrestep()
	{
		if ( !Game.IsServer )
			return;

		UpdateMovement();
	}

	public override void SetVehicleDefinition( BaseVehicleDefinition vehicleDefinition )
	{
		if ( vehicleDefinition.GetType() != typeof( WaterVehicleDefinition ) )
		{
			LOGGER.Error( $"WaterVehicleEntity had wrong vehicle definition set. Expected WaterVehicleDefinition recieved {vehicleDefinition.GetType().Name}" );
			return;
		}

		base.SetVehicleDefinition( vehicleDefinition );
		CreateFloaters();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		UpdateMovementInput();
	}

}
