﻿using Sandbox;

namespace TycoonGame.Player;

public partial class Player
{
	[Net]
	public Entity SelectedEntity { get; private set; }

	[ClientInput]
	public Vector2 InputCursorPosition { get; private set; }

	[ClientInput]
	public Vector2 InputHoveredWorldPosition { get; private set; }

	[ClientInput]
	public Entity InputHoveredEntity { get; private set; }

	public override void BuildInput()
	{
		base.BuildInput();

		InputCursorPosition = Mouse.Position;
		UpdateHoveredEntity();
		UpdateSelectedEntity();
		InputHoveredWorldPosition = GetHoveredWorldPosition();
	}

	public void SetSelectedEntity( Entity newSelectedEntity )
	{
		Game.AssertServer();

		if ( SelectedEntity != null && SelectedEntity is IInteractableEntity interactableEntity )
		{
			interactableEntity.Unselected( this );
			UnselectedEntityOnClient( To.Single( this ), SelectedEntity.NetworkIdent );
		}

		SelectedEntity = null;

		if ( newSelectedEntity is not IInteractableEntity newSelectedEntityInteraction )
			return;
		
		if ( newSelectedEntityInteraction.IsSelectable && newSelectedEntityInteraction.CanSelect( this ) )
		{
			newSelectedEntityInteraction.Selected( this );
			SelectedEntityOnClient( To.Single( this ), newSelectedEntity.NetworkIdent );
			SelectedEntity = newSelectedEntity;
		}
	}

	private void UpdateHoveredEntity()
	{
		var currentHoveredEntity = GetHoveredEntity();

		if ( InputHoveredEntity != currentHoveredEntity )
		{
			if (InputHoveredEntity != null && InputHoveredEntity is IInteractableEntity oldHoveredInteractableEntity)
			{
				oldHoveredInteractableEntity.Unhovered( this );
			}

			if ( currentHoveredEntity is IInteractableEntity newlyHoveredInteractableEntity && newlyHoveredInteractableEntity.IsSelectable )
			{
				InputHoveredEntity = currentHoveredEntity;
				newlyHoveredInteractableEntity.Hovered( this );
			} 
			else
			{
				InputHoveredEntity = null;
			}
		}
	}

	private void UpdateSelectedEntity()
	{
		if ( !Input.Pressed( InputButton.PrimaryAttack ) )
			return;

		if ( SelectedEntity == InputHoveredEntity )
			return;

		if (InputHoveredEntity == null)
		{
			ConCmd_SetSelectedEntity_Null();
		}
		else
		{
			ConCmd_SetSelectedEntity( InputHoveredEntity.NetworkIdent );
		}
	}

	private Vector2 GetHoveredWorldPosition()
	{
		Game.AssertClient();

		var ray = new Ray( Camera.Position, Screen.GetDirection( InputCursorPosition ) );
		var result = Trace.Ray( ray, 50_000f ).Run();
		return result.Hit ? new Vector2( result.EndPosition ) : Vector2.Zero;
	}

	private Entity GetHoveredEntity()
	{
		Game.AssertClient();

		var ray = new Ray( Camera.Position, Screen.GetDirection( InputCursorPosition ) );
		var result = Trace.Ray( ray, 50_000f ).EntitiesOnly().Run();
		return result.Hit ? result.Entity : null;
	}

	[ConCmd.Server]
	private static void ConCmd_SetSelectedEntity( int selectedEntityNetworkIdent )
	{
		if ( ConsoleSystem.Caller is null )
			return;

		var entity = FindByIndex( selectedEntityNetworkIdent );
		if ( entity == null )
			return;

		(ConsoleSystem.Caller.Pawn as Player).SetSelectedEntity( entity );
	}

	[ConCmd.Server]
	private static void ConCmd_SetSelectedEntity_Null()
	{
		if ( ConsoleSystem.Caller is null )
			return;

		(ConsoleSystem.Caller.Pawn as Player).SetSelectedEntity( null );
	}

	[ClientRpc]
	private void SelectedEntityOnClient( int networkId )
	{
		var entity = FindByIndex( networkId );
		if ( entity != null && entity is IInteractableEntity interactableEntity )
			interactableEntity.Selected( Game.LocalPawn as Player );
	}

	[ClientRpc]
	private void UnselectedEntityOnClient( int networkId )
	{
		var entity = FindByIndex( networkId );
		if ( entity != null && entity is IInteractableEntity interactableEntity )
			interactableEntity.Unselected( Game.LocalPawn as Player );
	}
}
