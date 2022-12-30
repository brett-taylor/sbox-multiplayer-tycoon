using Sandbox;
using TycoonGame.Vehicles.Definitions;

namespace TycoonGame.Vehicles;

public partial class VehicleGroupMember : BaseNetworkable
{
	[Net]
	public BaseVehicleDefinition VehicleDefinition { get; set; }
}
