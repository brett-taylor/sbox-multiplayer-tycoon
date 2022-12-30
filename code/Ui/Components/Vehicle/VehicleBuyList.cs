using System;
using System.Collections.Generic;
using System.Linq;
using TycoonGame.Building.Types.Interactable;
using TycoonGame.Vehicles;
using TycoonGame.Vehicles.Definitions;

public partial class VehicleBuyList
{
	public RoadDepot RoadDepot { get; set; }
	public List<BaseVehicleDefinition> BuyableVehicles { get; set; } = new List<BaseVehicleDefinition>();

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if (firstTime)
		{
			BuyableVehicles = ResourceLibrary.GetAll<BaseVehicleDefinition>().ToList();
		}
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( BuyableVehicles );
	}

	private void BuyVehicle( BaseVehicleDefinition vehicleDefinition )
	{
		RoadDepot.Concmd_BuyVehicle( RoadDepot.NetworkIdent, vehicleDefinition.ResourceId );
		StateHasChanged();
	}
}
