using TycoonGame.World;

namespace TycoonGame.Building.Restrictions;

public abstract class BuildingRestriction
{
	public abstract string LastFailedReason { get; }

	public abstract bool Applies( BuildingDefinition buildingDefinition );
	public abstract bool Valid( BuildingDefinition buildingDefinition, WorldCell worldCell );
}
