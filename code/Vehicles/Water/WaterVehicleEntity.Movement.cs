using Sandbox;
using System;

namespace TycoonGame.Vehicles.Water;

public partial class WaterVehicleEntity
{
	[Net]
	public bool DebugMovement { get; private set; } = false;

	[Net]
	public float InputThrottle { get; private set; } = 0f;

	[Net]
	public float InputTurning { get; private set; } = 0f;

	[Net]
	public float TurnDirection { get; private set; } = 0f;

	[Net]
	public float TurnLean { get; private set; } = 0f;

	[Net]
	public float Grip { get; private set; } = 0f;

	private WaterVehicleEntityFloater FloaterFrontLeft { get; set; }
	private WaterVehicleEntityFloater FloaterFrontRight { get; set; }
	private WaterVehicleEntityFloater FloaterBackLeft { get; set; }
	private WaterVehicleEntityFloater FloaterBackRight { get; set; }

	private void CreateFloaters()
	{
		FloaterFrontLeft = new WaterVehicleEntityFloater( this, new Vector2( 170f, - 70f ), 30f );
		FloaterFrontRight = new WaterVehicleEntityFloater( this, new Vector2( 170f, 70f ), 30f );
		FloaterBackLeft = new WaterVehicleEntityFloater( this, new Vector2( -250f, - 70f ), 30f );
		FloaterBackRight = new WaterVehicleEntityFloater( this, new Vector2( -250f, 70f ), 30f );
	}

	private void ResetInput()
	{
		InputThrottle = 0;
		InputTurning = 0;
	}

	private void UpdateMovementInput()
	{
		if ( !Game.IsServer )
			return;

		ResetInput();

		InputThrottle = (Input.Down( InputButton.Forward ) ? 1 : 0) + (Input.Down( InputButton.Back ) ? -1 : 0);
		InputTurning = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
	}

	private void UpdateMovement()
	{
		PhysicsBody.GravityEnabled = false;

		UpdateFloaters();

		ResetZPosition();

		UpdateTurnLean();

		SetPhysicalProperties();

		UpdateForwardBackwardsMovement();

		UpdateRotationalMovement();

		DrawDebug();
	}

	private void UpdateFloaters()
	{
		FloaterFrontLeft.DoPhysicsStep();
		FloaterFrontRight.DoPhysicsStep();
		FloaterBackLeft.DoPhysicsStep();
		FloaterBackRight.DoPhysicsStep();
	}

	private void ResetZPosition()
	{
		Position = new Vector3( Position.x, Position.y, -30f);
	}

	private bool IsCompletelyInWater()
	{
		return FloaterFrontLeft.IsOnWater && FloaterFrontRight.IsOnWater && FloaterBackLeft.IsOnWater && FloaterBackRight.IsOnWater;
	}

	private Vector3 MovementSpeed => PhysicsBody.Rotation.Inverse * PhysicsBody.Velocity;

	private float AccelerateDirection => InputThrottle.Clamp( -1, 1 );

	private float AccelerationIncreasePercentage => 1.0f - (MathF.Abs( MovementSpeed.x ) / 200f).Clamp( 0.0f, 1.0f );

	private void UpdateForwardBackwardsMovement()
	{
		PhysicsBody.DragEnabled = false;

		TurnDirection = TurnDirection.LerpTo( InputTurning.Clamp( -1, 1 ), 1.0f - MathF.Pow( 0.001f, Time.Delta ) );

		if ( IsCompletelyInWater() )
		{
			var directionAcceleration = 100f;
			var acceleration = AccelerationIncreasePercentage * directionAcceleration * AccelerateDirection * Time.Delta;
			var impulse = PhysicsBody.Rotation * new Vector3( acceleration, 0, 0 );
			PhysicsBody.Velocity += impulse;
		}
	}

	private void UpdateTurnLean()
	{
		float targetLean = 0;

		if ( IsCompletelyInWater() )
		{
			var forwardSpeed = MathF.Abs( MovementSpeed.x );
			var speedFraction = MathF.Min( forwardSpeed / 500.0f, 1 );

			targetLean = speedFraction * TurnDirection;
		}

		TurnLean = TurnLean.LerpTo( targetLean, 1.0f - MathF.Pow( 0.01f, Time.Delta ) );
	}

	private void UpdateRotationalMovement()
	{
		if ( IsCompletelyInWater() )
		{
			var turnAmount = MathF.Sign( MovementSpeed.x ) * 25.0f * CalculateTurnFactor( TurnDirection, MathF.Abs( MovementSpeed.x ) ) * Time.Delta;

			PhysicsBody.AngularVelocity += PhysicsBody.Rotation * new Vector3( 0, 0, turnAmount );

			var forwardGrip = 0.1f;
			PhysicsBody.Velocity = VelocityDamping( PhysicsBody.Velocity, PhysicsBody.Rotation, new Vector3( forwardGrip, Grip, 0 ), Time.Delta );
		}
	}

	private static float CalculateTurnFactor( float direction, float speed )
	{
		var turnFactor = MathF.Min( speed / 500.0f, 1 );
		var yawSpeedFactor = 1.0f - (speed / 1000.0f).Clamp( 0, 0.6f );

		return direction * turnFactor * yawSpeedFactor;
	}

	private static Vector3 VelocityDamping( Vector3 velocity, Rotation rotation, Vector3 damping, float dt )
	{
		var localVelocity = rotation.Inverse * velocity;
		var dampingPow = new Vector3( MathF.Pow( 1.0f - damping.x, dt ), MathF.Pow( 1.0f - damping.y, dt ), MathF.Pow( 1.0f - damping.z, dt ) );
		return rotation * (localVelocity * dampingPow);
	}

	private void SetPhysicalProperties()
	{
		var v = PhysicsBody.Rotation * MovementSpeed.WithZ( 0 );
		var vDelta = MathF.Pow( (v.Length / 1000.0f).Clamp( 0, 1 ), 5.0f ).Clamp( 0, 1 );
		if ( vDelta < 0.01f ) vDelta = 0;

		var angle = (PhysicsBody.Rotation.Forward.Normal * MathF.Sign( MovementSpeed.x )).Normal.Dot( v.Normal ).Clamp( 0.0f, 1.0f );
		angle = angle.LerpTo( 1.0f, 1.0f - vDelta );
		Grip = Grip.LerpTo( angle, 1.0f - MathF.Pow( 0.001f, Time.Delta ) );

		var angularDamping = 0.0f;
		angularDamping = angularDamping.LerpTo( 5.0f, Grip );

		PhysicsBody.LinearDamping = 0.0f;
		PhysicsBody.AngularDamping = IsCompletelyInWater() ? angularDamping : 0.5f;
	}

	private void DrawDebug()
	{
		if ( !DebugMovement )
			return;

		DebugOverlay.Line( PhysicsBody.MassCenter, PhysicsBody.MassCenter + PhysicsBody.Rotation.Forward.Normal * 100, Color.White, 0, false );

		FloaterFrontLeft.DrawDebug();
		FloaterFrontRight.DrawDebug();
		FloaterBackLeft.DrawDebug();
		FloaterBackRight.DrawDebug();
	}
}
