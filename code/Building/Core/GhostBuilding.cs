using Sandbox;
using TycoonGame.Building.Types;

namespace TycoonGame.Building.Core;

[Category( "Simulation" )]
public class GhostBuilding : BaseBuilding
{
	public override void Spawn()
	{
		base.Spawn();
		Game.AssertClient();

		Transmit = TransmitType.Never;
	}
}
