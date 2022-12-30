using Sandbox;
using TycoonGame.Simulation;
using TycoonGame.Vehicles;
using TycoonGame.World;

namespace TycoonGame;

/*
1. Vehicle driving ✅
2. Road building ✅
	2.1 Road Depot 
	2.2 Better Terrain with water
	2.3 Ships
	2.4 Ship Depot
3. Basic Industry chain
4. Rail building
5. Train driving
6. Industry chain via train
*/

public partial class TycoonGame : GameManager
{
	public static TycoonGame Instance { get; private set; }

	[Net]
	public WorldManager WorldManager { get; private set; }

	[Net]
	public CompanyManager CompanyManager { get; private set; }

	[Net]
	public VehicleManager VehicleManager { get; private set; }

	public TycoonGame()
	{
		if ( Game.IsClient )
			Game.RootPanel = new TycoonHud();

		if ( Game.IsServer )
		{
			WorldManager = new WorldManager();
			CompanyManager = new CompanyManager();
			VehicleManager = new VehicleManager();
		}

		Instance = this;
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Player.Player();
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		if (cl.Pawn!= null && cl.Pawn is Player.Player player )
			player.ClientDisconnected();

		CompanyManager.ClientDisconnect( cl, reason );
	}
}
