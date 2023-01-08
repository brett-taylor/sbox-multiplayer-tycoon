using Sandbox;
using TycoonGame.Building;

namespace TycoonGame.Simulation;

[Category( "Simulation" )]
public partial class CompanyManager : Entity
{
	[Net]
	public Player.Player Ceo { get; private set; }

	[Net]
	public int Money { get; private set; } = 70_000;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public void SetCeo( Player.Player player )
	{
		Ceo = player;
	}

	public void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		if ( cl.Pawn == Ceo )
			SetCeo( null );
	}

	public bool HasMoney( int amount )
	{
		return Money - amount > 0;
	}

	public void RemoveMoney( int amount )
	{
		Money -= amount;
	}
}
