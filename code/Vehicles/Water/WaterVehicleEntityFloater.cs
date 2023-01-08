using Sandbox;
using TycoonGame.Utilities.Enumertion;

namespace TycoonGame.Vehicles.Water;

public class WaterVehicleEntityFloater
{
	public bool IsOnWater { get; private set; }

	private WaterVehicleEntity WaterVehicleEntity { get; init; }

	private Vector2 Offset { get; init; }

	private float FloatHeightOffset { get; set; }

	public WaterVehicleEntityFloater( WaterVehicleEntity waterVehicleEntity, Vector3 offset, float floatHeightOffset )
	{
		WaterVehicleEntity = waterVehicleEntity;
		Offset = offset;
		FloatHeightOffset = floatHeightOffset;
	}

	public void DoPhysicsStep()
	{
		var tr = Trace.Ray( FloaterPosition, FloaterPositionWithOffset )
			.Ignore( WaterVehicleEntity )
			.WithTag( CustomTags.Water )
			.Run();

		IsOnWater = tr.Hit;
	}

	public void DrawDebug()
	{
		var color = IsOnWater ? Color.Green : Color.Red;
		
		DebugOverlay.Line( FloaterPosition, FloaterPositionWithOffset, color, 0F, false );
	}

	private Vector3 FloaterPosition => WaterVehicleEntity.Position + (	WaterVehicleEntity.Rotation * new Vector3( Offset, 0 ) );
	private Vector3 FloaterPositionWithOffset => FloaterPosition + ( WaterVehicleEntity.Rotation.Up * FloatHeightOffset );
}
