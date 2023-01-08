using System.Collections.Generic;
using System;
using TycoonGame.Building.Archetypes;
using TycoonGame.Vehicles;

public partial class VehicleDepotWithdrawList
{
	public VehicleDepot VehicleDepot { get; set; }
	public IList<VehicleGroup> StoredVehicles { get; set; } = new List<VehicleGroup>();

	protected override int BuildHash()
	{
		return HashCode.Combine( StoredVehicles );
	}

	private void WithdrawVehicle( VehicleGroup vehicleGroup )
	{
		VehicleDepot.Concmd_WithdrawVehicle( VehicleDepot.NetworkIdent, vehicleGroup.NetworkIdent );
		StateHasChanged();
	}
}
