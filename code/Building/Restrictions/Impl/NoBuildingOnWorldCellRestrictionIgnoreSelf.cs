using TycoonGame.Building.Archetypes;
using TycoonGame.Building.Definitions;
using TycoonGame.World.Data;

namespace TycoonGame.Building.Restrictions.Impl;

public class NoBuildingOnWorldCellRestrictionIgnoreSelf : BuildingRestriction
{
	public override string LastFailedReason => "Another building is already here";

	public override bool Applies( BuildingDefinition buildingDefinition ) => true;

	public override bool Valid( BuildingDefinition buildingDefinition, WorldCell worldCell )
	{
		if ( TycoonGame.Instance.WorldManager.DoesBuildingExistOn( worldCell.WorldCoordinate, out BaseBuilding building ) )
		{
			return building.BuildingDefinition == buildingDefinition;
		}
		
		return true;
	}
}
