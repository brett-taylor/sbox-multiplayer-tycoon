using Sandbox;
using Sandbox.Diagnostics;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.Vehicles.Base;
using TycoonGame.Vehicles.Definitions;

namespace TycoonGame.Vehicles.Road;

[Category( "Vehicles/RoadVehicles" )]
public partial class RoadVehicleEntity : BaseVehicleEntity
{
	private static readonly Logger LOGGER = new Logger( typeof( RoadVehicleEntity ).Name );

	public RoadVehicleDefinition RoadVehicleDefinition => VehicleDefinition as RoadVehicleDefinition;

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( CustomTags.RoadVehicle );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		
		CreatePhysicalWheels();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		UpdateMovementInput();
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
		if ( vehicleDefinition.GetType() != typeof( RoadVehicleDefinition ) )
		{
			LOGGER.Error( $"RoadVehicleEntity had wrong vehicle definition set. Expected RoadVehicleDefinition recieved {vehicleDefinition.GetType().Name}" );
			return;
		}

		base.SetVehicleDefinition( vehicleDefinition );

		CreateSimulatedWheels();
	}

	[Event.Client.Frame]
	private void OnFrame()
	{
		UpdatePhysicalWheels();
	}
}
