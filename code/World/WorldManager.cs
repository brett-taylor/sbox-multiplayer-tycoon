using Sandbox;
using Sandbox.Diagnostics;
using TycoonGame.Utilities;
using TycoonGame.World.Data;
using TycoonGame.World.Generator;

namespace TycoonGame.World;

[Category( "Simulation" )]
public partial class WorldManager : Entity
{
	private static readonly Logger LOGGER =  LoggerUtils.CreateLogger( typeof( WorldManager ) );

	[Net]
	public bool DebugWorldWaterServer { get; set; }

	[Net]
	public bool DebugWorldWaterClient { get; set; }

	public WorldCoordinate WorldSize { get; private set; }

	public float Seed { get; private set; }

	public WorldCell[,] WorldCells { get; private set; }

	
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

		CreatePhysicalWorld();

		if (Game.IsClient)
		{
			( Game.LocalPawn as Player.Player).TargetLookPosition = GetWorldCell( WorldSize / 2 ).CenterTilePosition();
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
}
