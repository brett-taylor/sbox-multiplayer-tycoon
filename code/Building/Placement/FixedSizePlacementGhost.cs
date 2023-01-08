using Sandbox;
using TycoonGame.Building.Archetypes;

namespace TycoonGame.Building.Placement;

[Category( "Simulation" )]
public class FixedSizePlacementGhost : BaseBuilding
{
	public override void Spawn()
	{
		base.Spawn();
		Game.AssertClient();

		Transmit = TransmitType.Never;
		EnableLagCompensation = false;
		Predictable = false;
	}
}
