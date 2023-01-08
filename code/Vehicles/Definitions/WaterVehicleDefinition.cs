using Sandbox;
using TycoonGame.Vehicles.Water;

namespace TycoonGame.Vehicles.Definitions;

[GameResource( "Water Vehicle Definition", "waterveh", "Water Vehicle" )]
public class WaterVehicleDefinition : BaseVehicleDefinition
{
	public override string EntityTypeName => typeof( WaterVehicleEntity ).Name;

	public float FloatHeight { get; set; }
}
