using TycoonGame.Building.Definitions;
using TycoonGame.Building.Restrictions;
using TycoonGame.World;

namespace Sandbox.Building.Restrictions.Impl;

public class IsOnGroundBuildingRestriction : BuildingRestriction
{
	public override string LastFailedReason => "Must be placed completely on the Ground";

	public override bool Applies( BuildingDefinition buildingDefinition ) => true;

	public override bool Valid( BuildingDefinition buildingDefinition, WorldCell worldCell ) => !worldCell.IsWater;
}
