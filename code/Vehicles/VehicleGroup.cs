using Sandbox;

namespace TycoonGame.Vehicles;

[Category( "Simulation/VehicleGroup" )]
public partial class VehicleGroup : Entity
{
	[Net]
	public VehicleGroupMember VehicleGroupMember { get; set; }

	[Net]
	public Entity DeployedEntity { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public bool IsDeployed()
	{
		return DeployedEntity != null;
	}
}
