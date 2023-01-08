using Sandbox;

namespace TycoonGame.Vehicles.Definitions;

public abstract class BaseVehicleDefinition : GameResource
{
	public abstract string EntityTypeName { get; }

	public string Name { get; set; }

	[ResourceType( "vmdl" )]
	public string ModelPath { get; set; }

	public int Price { get; set; }
}
