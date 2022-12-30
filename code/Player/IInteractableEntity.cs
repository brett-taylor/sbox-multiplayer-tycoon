namespace TycoonGame.Player;

public interface IInteractableEntity
{
	public abstract bool IsSelectable { get; }

	public abstract bool CanSelect( Player player );
	public abstract void Selected( Player player );
	public abstract void Unselected( Player player );
	public abstract void Hovered(Player player );
	public abstract void Unhovered( Player player );
}
