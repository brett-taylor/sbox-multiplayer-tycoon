using Sandbox;

namespace TycoonGame.Vehicles.Definitions;

[GameResource( "Road Vehicle Definition", "roadveh", "Road Vehicle" )]
public class RoadVehicleDefinition : BaseVehicleDefinition
{
	public float WheelSize { get; set; }

	public float MaximumSpeed { get; set; }

	public float AccelerationSpeed { get; set; }

	public float ReverseAccerlationSpeedModifier { get; set; }

	public Vector3 SimulatedLeftFrontWheelOffset { get; set; }

	public Vector3 SimulatedRightFrontWheelOffset { get; set; }

	public Vector3 SimulatedLeftBackWheelOffset { get; set; }

	public Vector3 SimulatedRightBackWheelOffset { get; set; }
}
