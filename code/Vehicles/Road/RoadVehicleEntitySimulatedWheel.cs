using Sandbox;
using System;
using TycoonGame.Utilities.Enumertion;

namespace TycoonGame.Vehicles.Road;

public class RoadVehicleEntitySimulatedWheel
{
	public bool IsGrounded { get; private set; }

	private RoadVehicleEntity RoadVehicle { get; init; }

	private Vector3 Offset { get; init; }

	private float PreviousLength { get; set; }

	private float CurrentLength { get; set; }

	public RoadVehicleEntitySimulatedWheel( RoadVehicleEntity roadVehicle, Vector3 offset )
	{
		RoadVehicle = roadVehicle;
		Offset = offset;
	}

	private Vector3 WheelPosition => RoadVehicle.Position + RoadVehicle.Rotation * Offset;

	public void DoPhysicsStep( float length )
	{
		var wheelExtend = WheelPosition - RoadVehicle.PhysicsBody.Rotation.Up * length;

		var tr = Trace.Ray( WheelPosition, wheelExtend )
			.Ignore( RoadVehicle )
			.WithTag( CustomTags.Solid )
			.Run();

		IsGrounded = tr.Hit;

		if ( IsGrounded )
		{
			PreviousLength = CurrentLength;
			CurrentLength = length - tr.Distance;

			var springVelocity = (CurrentLength - PreviousLength) / Time.Delta;
			var springForce = RoadVehicle.PhysicsBody.Mass * 50.0f * CurrentLength;
			var damperForce = RoadVehicle.PhysicsBody.Mass * (1.5f + (1.0f - tr.Fraction) * 3.0f) * springVelocity;
			var velocity = RoadVehicle.PhysicsBody.GetVelocityAtPoint( WheelPosition );
			var speed = velocity.Length;
			var speedDot = MathF.Abs( speed ) > 0.0f ? MathF.Abs( MathF.Min( Vector3.Dot( velocity, RoadVehicle.PhysicsBody.Rotation.Up.Normal ) / speed, 0.0f ) ) : 0.0f;
			var speedAlongNormal = speedDot * speed;
			var correctionMultiplier = (1.0f - tr.Fraction) * (speedAlongNormal / 1000.0f);
			var correctionForce = correctionMultiplier * 50.0f * speedAlongNormal / Time.Delta;

			RoadVehicle.PhysicsBody.ApplyImpulseAt( WheelPosition, tr.Normal * (springForce + damperForce + correctionForce) * Time.Delta );
		}
	}

	public void DrawDebug()
	{
		DebugOverlay.Sphere( WheelPosition, RoadVehicle.RoadVehicleDefinition.WheelSize, IsGrounded ? Color.Green : Color.Red, 0f, false );
	}
}
