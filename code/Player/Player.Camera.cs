using Sandbox;
using System.ComponentModel;

namespace TycoonGame.Player;

public partial class Player
{
	private static bool DEBUG_CAMERA = false;

	[EditorBrowsable]
	private float MinCameraHeight { get; set; } = 500f;
	
	[EditorBrowsable]
	private float MaxCameraHeight { get; set; } = 8000f;
	private float CameraPitch { get; set; } = 45f;
	private float Fov { get; set; } = 30f;
	private float PanSpeed { get; set; } = 2f;
	private float PanLerpTime { get; set; } = 5f;
	private float RotateSpeed { get; set; } = -0.025f;
	private float RotateLerpTime { get; set; } = 10f;
	private float ZoomSpeed { get; set; } = -0.05f;
	private float ZoomLerpTime { get; set; } = 10f;
	private float ZNear { get; set; } = 10f;
	private float ZFar { get; set; } = 25_000f;

	private Vector3 CurrentLookPosition { get; set; }
	private Vector3 TargetLookPosition { get; set; } = new Vector3( 400f, 400f, 0f );

	private float CurrentCameraYaw { get; set; }
	private float TargetCameraYaw { get; set; } = 45f;

	private float CurrentCameraZoom { get; set; }
	private float TargetCameraZoom { get; set; } = 0.5f;

	private Vector3? DragStartPosition { get; set; }

	private void InitCamera()
	{
		CurrentLookPosition = TargetLookPosition;
		CurrentCameraYaw = TargetCameraYaw;
		CurrentCameraZoom = TargetCameraZoom;
	}

	private void UpdateCamera()
	{
		Camera.FieldOfView = Fov;
		Camera.ZNear = ZNear;
		Camera.ZFar = ZFar;

		if ( SelectedEntity == null )
		{
			HandleKeyboardInput();
			HandleZoomAndRotationMouseInput();
			HandlePanMouseInput();
			PositionCamera();
		}
		else
		{
			HandleZoomAndRotationMouseInput();
			TargetLookPosition = SelectedEntity.Position.WithZ( 0f );
			PositionCamera();
		}

		if ( DEBUG_CAMERA )
		{
			DebugOverlay.Sphere( TargetLookPosition, 20f, Color.Blue, 0, false );
			DebugOverlay.Sphere( CurrentLookPosition, 20f, Color.Red, 0, false );

			if ( DragStartPosition.HasValue )
			{
				DebugOverlay.Sphere( DragStartPosition.Value, 20f, Color.Orange, 0, false );
			}
		}
	}


	private void HandleKeyboardInput()
	{
		var velocity = Vector3.Zero;

		if ( Input.Down( InputButton.Forward ) )
		{
			velocity += Rotation.FromYaw( CurrentCameraYaw ).Forward.WithZ( 0f ) * PanSpeed;
		}

		if ( Input.Down( InputButton.Back ) )
		{
			velocity += Rotation.FromYaw( CurrentCameraYaw ).Backward.WithZ( 0f ) * PanSpeed;
		}

		if ( Input.Down( InputButton.Left ) )
		{
			velocity += Rotation.FromYaw( CurrentCameraYaw ).Left.WithZ( 0f ) * PanSpeed;
		}

		if ( Input.Down( InputButton.Right ) )
		{
			velocity += Rotation.FromYaw( CurrentCameraYaw ).Right.WithZ( 0f ) * PanSpeed;
		}

		TargetLookPosition += velocity;
	}

	private void HandleZoomAndRotationMouseInput()
	{
		if ( Input.Down( InputButton.Zoom ) )
		{
			TargetCameraYaw += Input.MouseDelta.x * RotateSpeed;
		}

		TargetCameraZoom += Input.MouseWheel * ZoomSpeed;
		TargetCameraZoom = TargetCameraZoom.Clamp( 0f, 1f );
	}

	private void HandlePanMouseInput()
	{
		if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{
			DragStartPosition = InputHoveredWorldPosition;
		}

		if ( Input.Down( InputButton.SecondaryAttack ) )
		{
			var dragCurrentPosition = InputHoveredWorldPosition;

			var offset = new Vector2( DragStartPosition.Value.x - dragCurrentPosition.x, DragStartPosition.Value.y - dragCurrentPosition.y );
			TargetLookPosition = CurrentLookPosition + new Vector3( offset, 0f );
		}

		if ( Input.Released( InputButton.SecondaryAttack ) )
		{
			DragStartPosition = null;
		}
	}

	private void PositionCamera()
	{
		CurrentCameraZoom = MathX.Lerp( CurrentCameraZoom, TargetCameraZoom, ZoomLerpTime * Time.Delta );
		var cameraHeight = MathX.Lerp( MinCameraHeight, MaxCameraHeight, CurrentCameraZoom );

		CurrentCameraYaw = MathX.Lerp( CurrentCameraYaw, TargetCameraYaw, RotateLerpTime * Time.Delta );
		Camera.Rotation = Rotation.From( CameraPitch, CurrentCameraYaw, 0 );

		CurrentLookPosition = Vector3.Lerp( CurrentLookPosition, TargetLookPosition, PanLerpTime * Time.Delta );
		Camera.Position = CurrentLookPosition + (Vector3.Up * cameraHeight) + (Vector3.Forward * Rotation.FromYaw( CurrentCameraYaw ) * -cameraHeight);
	}
}
