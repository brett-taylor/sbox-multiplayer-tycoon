using Sandbox.Component;
using TycoonGame.Player;

namespace TycoonGame.Building.Types.Interactable;

public abstract class InteractableBuilding : BaseBuilding, IInteractableEntity
{
	private static readonly Color GLOW_COLOR = new Color32( 150, 182, 216 );

	public bool IsSelectable => true;

	public virtual bool CanSelect( Player.Player player )
	{
		return true;
	}

	public virtual void Selected( Player.Player player )
	{
	}

	public virtual void Unselected( Player.Player player )
	{
	}

	public virtual void Hovered( Player.Player player )
	{
		var glow = Components.GetOrCreate<Glow>();
		glow.Enabled = true;
		glow.Width = 0.5f;
		glow.Color = GLOW_COLOR;
	}

	public virtual void Unhovered( Player.Player player )
	{
		Components.GetOrCreate<Glow>().Enabled = false;
	}
}
