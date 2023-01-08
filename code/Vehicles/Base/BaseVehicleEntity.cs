using Sandbox;
using Sandbox.Component;
using Sandbox.Diagnostics;
using TycoonGame.Player;
using TycoonGame.Utilities;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.Vehicles.Definitions;

namespace TycoonGame.Vehicles.Base;

public abstract partial class BaseVehicleEntity : Prop, IInteractableEntity
{
	private static readonly Logger LOGGER = LoggerUtils.CreateLogger( typeof( BaseVehicleEntity ) );

	private static readonly Color GLOW_COLOR_SELECTABLE = Color.Green;
	private static readonly Color GLOW_COLOR_UNSELECTABLE = Color.Red;
	private static readonly Color GLOW_COLOR_SELECTED = new Color32( 150, 182, 216 );

	[Net]
	public BaseVehicleDefinition VehicleDefinition { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		Predictable = false;

		Tags.Add( CustomTags.Vehicle );
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

	public virtual void SetVehicleDefinition( BaseVehicleDefinition vehicleDefinition )
	{
		VehicleDefinition = vehicleDefinition;

		SetModel( vehicleDefinition.ModelPath );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		EnableSelfCollisions = false;
		EnableShadowCasting = false;
	}
}
