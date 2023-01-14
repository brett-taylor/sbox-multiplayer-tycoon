using TycoonGame.Building.Definitions;
using TycoonGame.Building.Restrictions;
using TycoonGame.World.Data;

namespace Sandbox.Building.Restrictions.Impl;

public class IsOnWaterBuildingRestriction : BuildingRestriction
{
	public override string LastFailedReason => "Must be placed completely in Water";

	public override bool Applies( BuildingDefinition buildingDefinition ) => true;

	public override bool Valid( BuildingDefinition buildingDefinition, WorldCell worldCell ) => worldCell.IsWater;
}
