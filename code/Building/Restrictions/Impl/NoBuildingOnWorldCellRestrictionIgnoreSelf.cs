using Sandbox;
using TycoonGame.Building.Archetypes;
using TycoonGame.Building.Definitions;
using TycoonGame.Utilities;

namespace TycoonGame.Building.Restrictions.Impl;

public class NoBuildingOnWorldCellRestrictionIgnoreSelf : NoBuildingOnWorldCellRestriction
{
	protected override bool FilterEntity( BuildingDefinition buildingDefinition, Entity entity )
	{
		return base.FilterEntity( buildingDefinition, entity ) && IsEntityBuildingDefinition( buildingDefinition, entity );
	}

	private bool IsEntityBuildingDefinition( BuildingDefinition buildingDefinition, Entity entity )
	{
		if (entity is BaseBuilding baseBuildingEntity) 
		{
			return baseBuildingEntity.BuildingDefinition != buildingDefinition;
		}
		
		return false;
	}
}
