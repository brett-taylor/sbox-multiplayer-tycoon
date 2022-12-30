using Sandbox;
using Sandbox.Component;
using Sandbox.Diagnostics;
using TycoonGame.Player;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.Vehicles.Definitions;

namespace TycoonGame.Vehicles.Road;

[Category( "Vehicles/RoadVehicles" )]
public partial class RoadVehicleEntity : Prop, IInteractableEntity
{
	private static readonly Logger LOGGER = new Logger( typeof( RoadVehicleEntity ).Name );

	private static readonly Color GLOW_COLOR_SELECTABLE = Color.Green;
	private static readonly Color GLOW_COLOR_UNSELECTABLE = Color.Red;
	private static readonly Color GLOW_COLOR_SELECTED = new Color32( 150, 182, 216 );

	[Net] 
	public RoadVehicleDefinition RoadVehicleDefinition { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Predictable = false;

		Tags.Add( CustomTags.Vehicle );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		
		CreatePhysicalWheels();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		UpdateMovementInput();
	}

	[Event.Physics.PreStep]
	public void PhysicsPrestep()
	{
		if ( !Game.IsServer )
			return;

		UpdateMovement();
	}

	public bool IsSelectable => true;

	public bool CanSelect( Player.Player player )
	{
		return Owner == null;
	}

	public virtual void Selected( Player.Player player )
	{
		if ( Game.IsServer && CanSelect( player ) )
		{
			LOGGER.Info( $"Player {player.Client.Name} has taken control of vehicle {Name}" );
			Owner = player;
		}

		if ( Game.IsClient && Owner == Game.LocalPawn )
		{
			Components.GetOrCreate<Glow>().Color = GLOW_COLOR_SELECTED;
		}
	}

	public virtual void Unselected( Player.Player player )
	{
		if ( Game.IsServer )
		{
			LOGGER.Info( $"Player {player.Client.Name} has relinquished control of vehicle {Name}" );
			Owner = null;
		}

		if ( Game.IsClient )
		{
			Components.GetOrCreate<Glow>().Enabled = false;
		}
	}

	public void Hovered( Player.Player player )
	{
		Game.AssertClient();

		if ( player == Game.LocalPawn && player != Owner )
		{
			var glow = Components.GetOrCreate<Glow>();
			glow.Enabled = true;
			glow.Width = 0.5f;
			glow.Color = CanSelect( player ) ? GLOW_COLOR_SELECTABLE : GLOW_COLOR_UNSELECTABLE;
		}
	}

	public void Unhovered( Player.Player player )
	{
		Game.AssertClient();

		if ( player == Game.LocalPawn && player != Owner )
		{
			Components.GetOrCreate<Glow>().Enabled = false;
		}
	}

	public void SetVehicleDefinition( RoadVehicleDefinition roadVehicleDefinition )
	{
		RoadVehicleDefinition = roadVehicleDefinition;

		SetModel( roadVehicleDefinition.ModelPath );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		EnableSelfCollisions = false;
		EnableShadowCasting = false;

		CreateSimulatedWheels();
	}

	[Event.Client.Frame]
	private void OnFrame()
	{
		UpdatePhysicalWheels();
	}
}
