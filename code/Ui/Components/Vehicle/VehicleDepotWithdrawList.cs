using System.Collections.Generic;
using System;
using TycoonGame.Building.Archetypes.Interactable;
using TycoonGame.Vehicles;

public partial class VehicleDepotWithdrawList
{
	public RoadDepot RoadDepot { get; set; }
	public IList<VehicleGroup> StoredVehicles { get; set; } = new List<VehicleGroup>();

	protected override int BuildHash()
	{
		return HashCode.Combine( StoredVehicles );
	}

	private void WithdrawVehicle( VehicleGroup vehicleGroup )
	{
		RoadDepot.Concmd_WithdrawVehicle( RoadDepot.NetworkIdent, vehicleGroup.NetworkIdent );
		StateHasChanged();
	}
}
