using Sandbox;
using System;
using TycoonGame.Utilities;

namespace TycoonGame.Vehicles.Road;

public partial class RoadVehicleEntity
{
	[Net]
	public bool DebugMovement { get; private set; } = false;

	[Net]
	public float MaximumTilt { get; private set; } = 2.5f;

	[Net]
	public float MaximumLean { get; private set; } = 2.5f;

	[Net]
	public float InputThrottle { get; private set; } = 0f;

	[Net]
	public float InputTurning { get; private set; } = 0f;

	[Net]
	public float InputBreaking { get; private set; } = 0f;

	[Net]
	public float TurnDirection { get; private set; } = 0f;

	[Net]
	public float AccelerationTilt { get; private set; } = 0f;

	[Net]
	public float TurnLean { get; private set; } = 0f;

	[Net]
	public float Grip { get; private set; } = 0f;

	private RoadVehicleEntitySimulatedWheel LeftFrontSimulatedWheel { get; set; }

	private RoadVehicleEntitySimulatedWheel RightFrontSimulatedWheel { get; set; }

	private RoadVehicleEntitySimulatedWheel LeftBackSimulatedWheel { get; set; }

	private RoadVehicleEntitySimulatedWheel RightBackSimulatedWheel { get; set; }

	private void CreateSimulatedWheels()
	{
		LeftFrontSimulatedWheel = new RoadVehicleEntitySimulatedWheel( this, RoadVehicleDefinition.SimulatedLeftFrontWheelOffset );
		RightFrontSimulatedWheel = new RoadVehicleEntitySimulatedWheel( this, RoadVehicleDefinition.SimulatedRightFrontWheelOffset );
		LeftBackSimulatedWheel = new RoadVehicleEntitySimulatedWheel( this, RoadVehicleDefinition.SimulatedLeftBackWheelOffset );
		RightBackSimulatedWheel = new RoadVehicleEntitySimulatedWheel( this, RoadVehicleDefinition.SimulatedRightBackWheelOffset );
	}

	private void ResetInput()
	{
		InputThrottle = 0;
		InputTurning = 0;
		InputBreaking = 0;
	}

	private void UpdateMovementInput()
	{
		if ( !Game.IsServer )
			return;

		ResetInput();

		InputThrottle = (Input.Down( InputButton.Forward ) ? 1 : 0) + (Input.Down( InputButton.Back ) ? -1 : 0);
		InputTurning = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
		InputBreaking = (Input.Down( InputButton.Jump ) ? 1 : 0);
	}

	private void UpdateMovement()
	{
		if ( Owner == null)
		{
			ResetInput();
			InputBreaking = 30f;
		}

		UpdateInputBreaking();

		UpdateForwardBackwardsMovement();

		UpdateTiltAndLean();
		
		UpdateWheels();

		SetPhysicalProperties();

		UpdateRotationalMovement();

		DrawDebug();
	}

	private Vector3 MovementSpeed => PhysicsBody.Rotation.Inverse * PhysicsBody.Velocity;

	private float AccelerateDirection => InputThrottle.Clamp( -1, 1 ) * (1.0f - InputBreaking);

	private float AccelerationIncreasePercentage => 1.0f - (MathF.Abs( MovementSpeed.x ) / RoadVehicleDefinition.MaximumSpeed).Clamp( 0.0f, 1.0f );

	private void UpdateInputBreaking()
	{
		var applyBreaking = MovementSpeed.x > 50f && InputThrottle < 0f || MovementSpeed.x < 50f && InputThrottle > 0f;
		var amountToApply = applyBreaking ? 3f * (1f - AccelerationIncreasePercentage) : 0;

		InputBreaking = Math.Max( InputBreaking, amountToApply );

		if ( InputThrottle == 0f && InputBreaking == 0f )
			InputBreaking = 0.5f;
	}

	private void UpdateForwardBackwardsMovement()
	{
		PhysicsBody.DragEnabled = false;

		TurnDirection = TurnDirection.LerpTo( InputTurning.Clamp( -1, 1 ), 1.0f - MathF.Pow( 0.001f, Time.Delta ) );

		if ( IsAnyPowerWheelOnGround() )
		{
			var directionAcceleration = AccelerateDirection < 0.0f ? RoadVehicleDefinition.AccelerationSpeed * RoadVehicleDefinition.ReverseAccerlationSpeedModifier : RoadVehicleDefinition.AccelerationSpeed;
			var acceleration = AccelerationIncreasePercentage * directionAcceleration * AccelerateDirection * Time.Delta;
			var impulse = PhysicsBody.Rotation * new Vector3( acceleration, 0, 0 );
			PhysicsBody.Velocity += impulse;
		}
	}

	private void UpdateTiltAndLean()
	{
		float targetTilt = 0;
		float targetLean = 0;

		if ( IsAnyWheelOnGround() )
		{
			var forwardSpeed = MathF.Abs( MovementSpeed.x );
			var speedFraction = MathF.Min( forwardSpeed / 500.0f, 1 );

			targetTilt = AccelerateDirection.Clamp( -1.0f, 1.0f );
			targetLean = speedFraction * TurnDirection;
		}

		AccelerationTilt = AccelerationTilt.LerpTo( targetTilt, 1.0f - MathF.Pow( 0.01f, Time.Delta ) );
		TurnLean = TurnLean.LerpTo( targetLean, 1.0f - MathF.Pow( 0.01f, Time.Delta ) );
	}

	private void UpdateWheels()
	{
		var tiltAmount = AccelerationTilt * MaximumTilt;
		var leanAmount = TurnLean * MaximumLean;

		LeftFrontSimulatedWheel.DoPhysicsStep( RoadVehicleDefinition.WheelSize + tiltAmount - leanAmount );
		RightFrontSimulatedWheel.DoPhysicsStep( RoadVehicleDefinition.WheelSize + tiltAmount + leanAmount );
		LeftBackSimulatedWheel.DoPhysicsStep( RoadVehicleDefinition.WheelSize - tiltAmount - leanAmount );
		RightBackSimulatedWheel.DoPhysicsStep( RoadVehicleDefinition.WheelSize - tiltAmount + leanAmount );
	}

	private bool AreAllWheelsOnGound()
	{
		var frontWheels = LeftFrontSimulatedWheel.IsGrounded || RightFrontSimulatedWheel.IsGrounded;
		var backWheels = LeftBackSimulatedWheel.IsGrounded || RightBackSimulatedWheel.IsGrounded;
		return frontWheels && backWheels;
	}

	private bool IsAnyWheelOnGround()
	{
		var frontWheels = LeftFrontSimulatedWheel.IsGrounded || RightFrontSimulatedWheel.IsGrounded;
		var backWheels = LeftBackSimulatedWheel.IsGrounded || RightBackSimulatedWheel.IsGrounded;
		return frontWheels || backWheels;
	}

	private bool IsAnyPowerWheelOnGround() => LeftBackSimulatedWheel.IsGrounded || RightBackSimulatedWheel.IsGrounded;

	private bool IsAnySteeringWheelOnGround() => LeftFrontSimulatedWheel.IsGrounded || RightFrontSimulatedWheel.IsGrounded;

	private void SetPhysicalProperties()
	{
		if ( AreAllWheelsOnGound() )
		{
			PhysicsBody.Velocity += Game.PhysicsWorld.Gravity * Time.Delta;
		}

		PhysicsBody.GravityScale = AreAllWheelsOnGound() ? 0 : 1;

		var v = PhysicsBody.Rotation * MovementSpeed.WithZ( 0 );
		var vDelta = MathF.Pow( (v.Length / 1000.0f).Clamp( 0, 1 ), 5.0f ).Clamp( 0, 1 );
		if ( vDelta < 0.01f ) vDelta = 0;

		var angle = (PhysicsBody.Rotation.Forward.Normal * MathF.Sign( MovementSpeed.x )).Normal.Dot( v.Normal ).Clamp( 0.0f, 1.0f );
		angle = angle.LerpTo( 1.0f, 1.0f - vDelta );
		Grip = Grip.LerpTo( angle, 1.0f - MathF.Pow( 0.001f, Time.Delta ) );

		var angularDamping = 0.0f;
		angularDamping = angularDamping.LerpTo( 5.0f, Grip );

		PhysicsBody.LinearDamping = 0.0f;
		PhysicsBody.AngularDamping = AreAllWheelsOnGound() ? angularDamping : 0.5f;
	}

	private void UpdateRotationalMovement()
	{
		if ( IsAnyWheelOnGround() )
		{
			var turnAmount = IsAnySteeringWheelOnGround()
				? MathF.Sign( MovementSpeed.x ) * 25.0f * CalculateTurnFactor( TurnDirection, MathF.Abs( MovementSpeed.x ) ) * Time.Delta
				: 0f;

			PhysicsBody.AngularVelocity += PhysicsBody.Rotation * new Vector3( 0, 0, turnAmount );

			var forwardGrip = 0.1f;
			forwardGrip = forwardGrip.LerpTo( 0.9f, InputBreaking );
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

	private void DrawDebug()
	{
		if ( !DebugMovement )
			return;

		DebugOverlay.Line( PhysicsBody.MassCenter, PhysicsBody.MassCenter + PhysicsBody.Rotation.Forward.Normal * 100, Color.White, 0, false );
		DebugOverlay.Line( PhysicsBody.MassCenter, PhysicsBody.MassCenter + (PhysicsBody.Rotation * MovementSpeed.WithZ( 0 )).Normal * 100, Color.Green, 0, false );

		LeftFrontSimulatedWheel.DrawDebug();
		RightFrontSimulatedWheel.DrawDebug();
		LeftBackSimulatedWheel.DrawDebug();
		RightBackSimulatedWheel.DrawDebug();

		DebugOverlay.ScreenText( $"Speed {MovementSpeed.x}", new Vector2( 150f, 20f ), 0, Color.White, 0f );
		DebugOverlay.ScreenText( $"Acceleration {AccelerationIncreasePercentage}", new Vector2( 150f, 20f ), 1, Color.White, 0f );
	}
}
