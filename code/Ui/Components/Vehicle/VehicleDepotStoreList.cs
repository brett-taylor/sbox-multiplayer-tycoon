using System;
using System.Collections.Generic;
using TycoonGame.Building.Archetypes;
using TycoonGame.Vehicles;

public partial class VehicleDepotStoreList
{
	public VehicleDepot VehicleDepot { get; set; }
	public IList<VehicleGroup> StorableVehicles { get; set; } = new List<VehicleGroup>();

	protected override int BuildHash()
	{
		return HashCode.Combine( StorableVehicles );
	}

	private void StoreVehicle( VehicleGroup vehicleGroup )
	{
		VehicleDepot.Concmd_StoreVehicle( VehicleDepot.NetworkIdent, vehicleGroup.NetworkIdent );
		StateHasChanged();
	}
}
