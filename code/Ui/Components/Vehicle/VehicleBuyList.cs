using System;
using System.Collections.Generic;
using System.Linq;
using TycoonGame.Building.Archetypes;
using TycoonGame.Vehicles;
using TycoonGame.Vehicles.Definitions;

public partial class VehicleBuyList
{
	public VehicleDepot RoadDepot { get; set; }
	public VehicleGroupType VehicleType { get; set; }

	private List<BaseVehicleDefinition> BuyableVehicles { get; set; } = new List<BaseVehicleDefinition>();

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if (firstTime)
		{
			BuyableVehicles = ResourceLibrary
				.GetAll<BaseVehicleDefinition>()
				.Where( bvd => VehicleType == bvd.Type )
				.ToList();
		}
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( BuyableVehicles );
	}

	private void BuyVehicle( BaseVehicleDefinition vehicleDefinition )
	{
		VehicleDepot.Concmd_BuyVehicle( RoadDepot.NetworkIdent, vehicleDefinition.ResourceId );
		StateHasChanged();
	}
}
