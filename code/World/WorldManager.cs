using Sandbox;

namespace TycoonGame.World;

[Category( "Simulation" )]
public partial class WorldManager : Entity
{
	public static Vector2 TILE_SIZE_IN_UNITS => new( 500f, 500f );
	public static Vector2 WORLD_SIZE => new( 100f, 100f );

	[Net]
	public bool DebugWorld { get; private set; }

	[Net]
	private WorldEntity WorldEntity { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		CreateWorldEntity();
	}

	public static Vector2 GetWorldSizeInUnits() => new Vector2(WORLD_SIZE.x * TILE_SIZE_IN_UNITS.x,WORLD_SIZE.y * TILE_SIZE_IN_UNITS.y);

	public static Vector2 GetTileFromWorldPosition(Vector2 worldPosition)
	{
		return new Vector2(
			MathX.Floor(worldPosition.x / TILE_SIZE_IN_UNITS.x),
			MathX.Floor(worldPosition.y / TILE_SIZE_IN_UNITS.y)
		);
	}

	private void CreateWorldEntity()
	{
		if (WorldEntity != null)
		{
			WorldEntity.Delete();
			WorldEntity = null;
		}

		WorldEntity = new WorldEntity();
		WorldEntity.Position = new Vector3( 0f, 0f, 0f );
	}

	[Event.Client.Frame]
	private void PositionUnderHoveredCursor()
	{
		if ( !DebugWorld )
			return;

		for (int y = 0; y < WORLD_SIZE.y; y++)
		{
			for (int x = 0; x < WORLD_SIZE.x; x++ )
			{
				Vector3 mins = new Vector3( x * TILE_SIZE_IN_UNITS.x, y * TILE_SIZE_IN_UNITS.y, 0f);
				Vector3 maxs = new Vector3( mins.x + TILE_SIZE_IN_UNITS.x, mins.y + TILE_SIZE_IN_UNITS.y, 1f );
				DebugOverlay.Box( mins, maxs, Color.Red, 0f, false );
			}
		}
	}
}
