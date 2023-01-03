using Sandbox;
using Sandbox.Diagnostics;
using TycoonGame.Utilities;
using TycoonGame.World.Physical;

namespace TycoonGame.World;

[Category( "Simulation" )]
public partial class WorldManager : Entity
{
	private static readonly Logger LOGGER =  LoggerUtils.CreateLogger( typeof( WorldManager ) );

	[Net]
	public bool DebugWorldServer { get; set; }

	[Net]
	public bool DebugWorldClient { get; set; }

	public WorldCoordinate WorldSize { get; private set; }

	public float Seed { get; private set; }

	public WorldCell[,] WorldCells { get; private set; }

	public WorldGroundEntity WorldGroundEntity { get; private set; }
	public WorldWaterEntity WorldWaterEntity { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	public void CreateWorld( WorldCoordinate size, float seed )
	{
		WorldSize = size;
		Seed = seed;

		GenerateWorldCells();

		WorldGroundEntity = CreateWorldGroundEntity();
		WorldWaterEntity = CreateWorldWaterEntity();

		if (Game.IsClient)
		{
			CreateWorldBedrockEntity();
			(Game.LocalPawn as Player.Player).TargetLookPosition = GetWorldCell( WorldSize / 2 ).CenterTilePosition();
		}
	}

	public WorldCell GetWorldCell( WorldCoordinate worldCoordinate )
	{
		if ( worldCoordinate.X < 0 || worldCoordinate.Y < 0 || worldCoordinate.X > WorldSize.X - 1 || worldCoordinate.Y > WorldSize.Y - 1 )
		{
			return null;
		}

		return WorldCells[worldCoordinate.X, worldCoordinate.Y];
	}

	public WorldCell GetWorldCellFromWorldPosition( Vector3 position ) 
	{
		var x = MathX.FloorToInt( position.x / WorldCell.WORLD_CELL_SIZE );
		var y = MathX.FloorToInt( position.y / WorldCell.WORLD_CELL_SIZE );
		return GetWorldCell( new WorldCoordinate( x, y ) );
	}

	private void GenerateWorldCells()
	{
		LOGGER.Info( $"Creating world with size {WorldSize} and seed {Seed}" );

		var generator = new WorldGenerator();
		generator.WorldSize = WorldSize;
		generator.Seed = Seed;

		WorldCells = generator.GenerateProceduralWorld();
	}

	private WorldGroundEntity CreateWorldGroundEntity()
	{
		var worldGround = new WorldGroundEntity();
		worldGround.Position = new Vector3( 0f, 0f, 0f );

		if ( Game.IsServer )
		{
			worldGround.Name = "WorldGroundEntity (Server)";
		}
		else
		{
			worldGround.Name = "WorldGroundEntity (Client)";
			worldGround.Owner = Game.LocalPawn;
		}

		return worldGround;
	}

	private WorldWaterEntity CreateWorldWaterEntity()
	{
		var worldWater = new WorldWaterEntity();
		worldWater.Position = new Vector3( 0f, 0f, 0f );

		if (Game.IsServer)
		{
			worldWater.Name = "WorldWaterEntity (Server)";
		}
		else
		{
			worldWater.Name = "WorldWaterEntity (Client)";
			worldWater.Owner = Game.LocalPawn;
		}

		return worldWater;
	}

	private WorldBedrockEntity CreateWorldBedrockEntity()
	{
		Assert.True( Game.IsClient );

		var worldBedrock = new WorldBedrockEntity();
		worldBedrock.Position = new Vector3( 0f, 0f, 0f );
		worldBedrock.Name = $"WorldBedrockEntity (Client)";
		worldBedrock.Owner = Game.LocalPawn;

		return worldBedrock;
	}

	[Event.Tick.Client]
	private void TickClient()
	{
		if ( !DebugWorldClient )
			return;

		for ( var x = 0; x < WorldSize.X; x++ )
		{
			for ( var y = 0; y < WorldSize.Y; y++ )
			{
				var color = WorldCells[x, y].IsWater ? Color.Blue : Color.Green;
				DebugOverlay.Box( WorldCells[x, y].BoundingBox().Mins, WorldCells[x, y].BoundingBox().Maxs, color, 0f, false );
				DebugOverlay.Sphere( WorldCells[x, y].CenterTilePosition(), 10f, color, 0f, false );
			}
		}
	}

	[Event.Tick.Server]
	private void TickServer()
	{
		if ( !DebugWorldServer)
			return;

		for ( var x = 0; x < WorldSize.X; x++ )
		{
			for ( var y = 0; y < WorldSize.Y; y++ )
			{
				var color = WorldCells[x, y].IsWater ? Color.Blue : Color.Green;
				DebugOverlay.Box( WorldCells[x, y].BoundingBox().Mins, WorldCells[x, y].BoundingBox().Maxs, color, 0f, false );
				DebugOverlay.Sphere( WorldCells[x, y].CenterTilePosition(), 10f, color, 0f, false );
			}
		}
	}
}
