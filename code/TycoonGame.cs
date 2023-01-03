using Sandbox;
using System;
using TycoonGame.Simulation;
using TycoonGame.Utilities;
using TycoonGame.Vehicles;
using TycoonGame.World;

namespace TycoonGame;

/*
1. Vehicle driving ✅
2. Road building ✅
	2.1 Road Depot ✅
	2.2 Better Terrain with water ✅
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
		Instance = this;

		if ( Game.IsClient )
		{
			Game.RootPanel = new TycoonHud(); 
		}

		if ( Game.IsServer )
		{
			WorldManager = new WorldManager();
			var seed = new Random().Float( -1f, 1f ) * 50_00f;
			WorldManager.CreateWorld( new WorldCoordinate( 20, 20 ), seed );

			CompanyManager = new CompanyManager();
			VehicleManager = new VehicleManager();
		}
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		cl.Pawn = new Player.Player();

		CreateWorldClient( To.Single( cl ), WorldManager.WorldSize.X, WorldManager.WorldSize.Y, WorldManager.Seed );
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		if (cl.Pawn!= null && cl.Pawn is Player.Player player )
			player.ClientDisconnected();

		CompanyManager.ClientDisconnect( cl, reason );
	}


	[ClientRpc]
	private void CreateWorldClient(int worldSizeX, int worldSizeY, float seed)
	{
		WorldManager.CreateWorld( new WorldCoordinate( worldSizeX, worldSizeY ), seed );
	}
}
