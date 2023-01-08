using TycoonGame.Building.Archetypes;

namespace Sandbox.Building.Archetypes;

[Category( "Buildings/Infrastructure/Roads" )]
public partial class Road : BaseBuilding
{
	public override bool IsSelectable => false;
}
