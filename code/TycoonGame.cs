using Sandbox;
using System;
using TycoonGame.Simulation;
using TycoonGame.Vehicles;
using TycoonGame.World;

namespace TycoonGame;

/*
1. Vehicle driving ✅
2. Road building ✅
3. Road Depot ✅
4. Better Terrain with water ✅
5. Ships ✅
6. Ship Depot (With ground/water restriction in building controller) ✅
7. Road dragging and tiles updating (Rough system, needs whole Track system that will come with rails)✅
8. Basic Industry chain (Producer building, vehicle can pick up resource, drop to consumer, get money, no producer/supplier mix yet)
9. Rail dragging building. (Probably just straight tracks and corners, no junctions yet)
10. Trains and driving (Multi entity vehicles)
11. Any necessary updates to industry chains to make trains work (Train stations at least?)
12. Rail Junctions
13. Rail Signals
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
		Instance = this;

		if ( Game.IsClient )
		{
			Game.RootPanel = new TycoonHud(); 
		}

		if ( Game.IsServer )
		{
			WorldManager = new WorldManager();
			var seed = new Random().Float( -1f, 1f ) * 50_00f;
			seed = 696.17926f;
			WorldManager.CreateWorld( new WorldCoordinate( 50, 50 ), seed );

			CompanyManager = new CompanyManager();
			VehicleManager = new VehicleManager();
		}
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Player.Player();

		CreateWorldClient( To.Single( cl ), WorldManager.WorldSize.X, WorldManager.WorldSize.Y, WorldManager.Seed );
		WorldManager.RefreshBuildingsForNewClient( cl );
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		if (cl.Pawn!= null && cl.Pawn is Player.Player player )
			player.ClientDisconnected();

		CompanyManager.ClientDisconnect( cl, reason );
	}

	[ClientRpc]
	private void CreateWorldClient( int worldSizeX, int worldSizeY, float seed )
	{
		WorldManager.CreateWorld( new WorldCoordinate( worldSizeX, worldSizeY ), seed );
	}
}
