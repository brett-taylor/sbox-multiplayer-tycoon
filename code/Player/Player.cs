using Sandbox;
using TycoonGame.Building;

namespace TycoonGame.Player;

public partial class Player : Entity
{
	[Net, Local]
	public BuildingController BuildingController { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		if ( !Game.IsServer )
			return;

		Transmit = TransmitType.Always;

		BuildingController = new BuildingController();
		BuildingController.Owner = this;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		InitCamera();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( SelectedEntity != null )
		{
			SelectedEntity.Simulate( cl );
		}

		BuildingController.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		UpdateCamera();

		if ( SelectedEntity != null )
		{
			SelectedEntity.FrameSimulate( cl );
		}

		BuildingController.FrameSimulate( cl );
	}

	public void ClientDisconnected()
	{
		SetSelectedEntity( null );	
	}
}
