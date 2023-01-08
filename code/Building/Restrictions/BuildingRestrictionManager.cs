using Sandbox;
using Sandbox.Building.Restrictions.Impl;
using System.Collections.Generic;
using System.Linq;
using TycoonGame.Building.Definitions;
using TycoonGame.Building.Restrictions.Impl;
using TycoonGame.World;

namespace TycoonGame.Building.Restrictions;

[SkipHotload]
public class BuildingRestrictionManager : IHotloadManaged
{
	public static BuildingRestrictionManager Instance => GetInstance();
	
	private static BuildingRestrictionManager instance;

	private static BuildingRestrictionManager GetInstance()
	{
		if (instance == null)
		{
			instance = new BuildingRestrictionManager();
		}  
		
		return instance;
	}

	private Dictionary<BuildingRestrictionType, BuildingRestriction> Restrictions { get; set; }

	private BuildingRestrictionManager()
	{
		CreateRestrictions();
	}

	public bool IsValid( BuildingDefinition buildingDefinition, WorldCell worldCell, out BuildingRestriction buildingRestriction)
	{
		var restrictionsToApply = GetRestrictionsToApply( buildingDefinition );
		
		foreach( var restriction in restrictionsToApply ) 
		{
			if ( !restriction.Valid( buildingDefinition, worldCell ) )
			{
				buildingRestriction = restriction;
				return false;
			}
		}

		buildingRestriction = null;
		return true;
	}

	private List<BuildingRestriction> GetRestrictionsToApply(BuildingDefinition buildingDefinition)
	{
		return buildingDefinition.Restrictions
			.Select( restrictionType => Restrictions[restrictionType] )
			.Where( restriction => restriction.Applies( buildingDefinition ) )
			.ToList();
	}

	private void CreateRestrictions()
	{
		Restrictions = new Dictionary<BuildingRestrictionType, BuildingRestriction>
		{
			{ BuildingRestrictionType.IS_ON_GROUND, new IsOnGroundBuildingRestriction() },
			{ BuildingRestrictionType.IS_ON_WATER, new IsOnWaterBuildingRestriction() },
			{ BuildingRestrictionType.NO_BUILDING_ON_WORLD_CELL, new NoBuildingOnWorldCellRestriction() }
		};
	}

	public void Created( IReadOnlyDictionary<string, object> state )
	{
		CreateRestrictions();
	}
}
