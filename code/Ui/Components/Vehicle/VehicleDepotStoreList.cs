using System;
using System.Collections.Generic;
using TycoonGame.Building.Archetypes.Interactable;
using TycoonGame.Vehicles;

public partial class VehicleDepotStoreList
{
	public RoadDepot RoadDepot { get; set; }
	public IList<VehicleGroup> StorableVehicles { get; set; } = new List<VehicleGroup>();

	protected override int BuildHash()
	{
		return HashCode.Combine( StorableVehicles );
	}

	private void StoreVehicle( VehicleGroup vehicleGroup )
	{
		RoadDepot.Concmd_StoreVehicle( RoadDepot.NetworkIdent, vehicleGroup.NetworkIdent );
		StateHasChanged();
	}
}
