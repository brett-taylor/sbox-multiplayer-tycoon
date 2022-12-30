using Sandbox;
using System;

namespace TycoonGame.Vehicles.Road;

public partial class RoadVehicleEntity
{
	[Net]
	public Vector3 PhysicalLeftFrontWheelOffset { get; private set; } = new Vector3( 51f, 32f, 0f );

	[Net]
	public Vector3 PhysicalRightFrontWheelOffset { get; private set; } = new Vector3( 51f, -32f, 0f );

	[Net]
	public Vector3 PhysicalLeftBackWheelOffset { get; private set; } = new Vector3( -30f, 32f, 0f );

	[Net]
	public Vector3 PhysicalRightBackWheelOffset { get; private set; } = new Vector3( -30f, -32f, 0f );

	[Net]
	public float MaximumWheelTurnAngle { get; private set; } = 30f;

	private float WheelAngle { get; set; } = 0f;

	private ModelEntity FrontLeftPhysicalWheel { get; set; }

	private ModelEntity FrontRightPhysicalWheel { get; set; }

	private ModelEntity BackLeftPhysicalWheel { get; set; }

	private ModelEntity BackRightPhysicalWheel { get; set; }

	private void CreatePhysicalWheels()
	{
		FrontLeftPhysicalWheel = CreatePhysicalWheel( PhysicalLeftFrontWheelOffset, false );
		FrontRightPhysicalWheel = CreatePhysicalWheel( PhysicalRightFrontWheelOffset, true );
		BackLeftPhysicalWheel = CreatePhysicalWheel( PhysicalLeftBackWheelOffset, false );
		BackRightPhysicalWheel = CreatePhysicalWheel( PhysicalRightBackWheelOffset, true );
	}

	private ModelEntity CreatePhysicalWheel( Vector3 offset, bool flip )
	{
		var wheel = new ModelEntity();
		wheel.SetModel( "models/vehicles/wheels/truck_i_wheel.vmdl" );
		wheel.Transform = Transform;
		wheel.Parent = this;
		wheel.LocalPosition = offset;
		wheel.LocalRotation = flip ? Rotation.From( 0f, 180f, 0f ) : Rotation.From( 0f, 0f, 0f );
		
		wheel.EnableShadowCasting= false;

		return wheel;
	}

	private void UpdatePhysicalWheels()
	{
		UpdatePhysicalWheelPositions();
		UpdatePhysicalWheelRotations();
	}

	private void UpdatePhysicalWheelPositions()
	{
		FrontLeftPhysicalWheel.LocalPosition = PhysicalLeftFrontWheelOffset;
		FrontRightPhysicalWheel.LocalPosition = PhysicalRightFrontWheelOffset;
		BackLeftPhysicalWheel.LocalPosition = PhysicalLeftBackWheelOffset;
		BackRightPhysicalWheel.LocalPosition = PhysicalRightBackWheelOffset;
	}

	private void UpdatePhysicalWheelRotations()
	{
		WheelAngle = WheelAngle.LerpTo( TurnDirection * MaximumWheelTurnAngle, 1.0f - MathF.Pow( 0.001f, Time.Delta ) );

		FrontLeftPhysicalWheel.LocalRotation = Rotation.FromYaw( WheelAngle );
		FrontRightPhysicalWheel.LocalRotation = Rotation.FromYaw( 180f + WheelAngle );
	}
}
