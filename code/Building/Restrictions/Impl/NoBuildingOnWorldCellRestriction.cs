using Sandbox;
using System.Linq;
using TycoonGame.Building.Definitions;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.World;

namespace TycoonGame.Building.Restrictions.Impl;

public class NoBuildingOnWorldCellRestriction : BuildingRestriction
{
	private static float BBOX_DEADZONE = 50f;

	public override string LastFailedReason => "Another building is already here";

	public override bool Applies( BuildingDefinition buildingDefinition ) => true;

	public override bool Valid( BuildingDefinition buildingDefinition, WorldCell worldCell )
	{
		var mins = new Vector3( worldCell.BottomLeftPosition().x + BBOX_DEADZONE, worldCell.BottomLeftPosition().y + BBOX_DEADZONE, 0f );
		var maxs = new Vector3( worldCell.TopRightPosition().x - BBOX_DEADZONE, worldCell.TopRightPosition().y - BBOX_DEADZONE, 50f );
		var bboxToCheck = new BBox( mins, maxs );
		var entities = Entity.FindInBox( bboxToCheck ).Where( entity => entity.Tags.Has( CustomTags.Building ) ).ToList();

		return entities.Count == 0;
	}
}
