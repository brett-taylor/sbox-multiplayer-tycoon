using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace TycoonGame.World.Data;

public class WorldCell
{
	public static readonly float WORLD_CELL_SIZE = 500f;
	public static readonly float WORLD_CELL_HEIGHT = 50f;

	public WorldCoordinate WorldCoordinate { get; set; }

	public bool IsWater { get; set; }

	public int WorldHeight() => IsWater ? 0 : 1;

	public BBox BoundingBox() => new( BottomLeftPosition(), BottomRightPosition() );
	public Vector3 CenterTilePosition() => new( WorldCoordinate.X * WORLD_CELL_SIZE + WORLD_CELL_SIZE / 2f, WorldCoordinate.Y * WORLD_CELL_SIZE + WORLD_CELL_SIZE / 2f, WorldHeight() * WORLD_CELL_HEIGHT );
	public Vector3 BottomLeftPosition() => new( WorldCoordinate.X * WORLD_CELL_SIZE, WorldCoordinate.Y * WORLD_CELL_SIZE, WorldHeight() * WORLD_CELL_HEIGHT );
	public Vector3 BottomRightPosition() => new( WorldCoordinate.X * WORLD_CELL_SIZE, WorldCoordinate.Y * WORLD_CELL_SIZE + WORLD_CELL_SIZE, WorldHeight() * WORLD_CELL_HEIGHT );
	public Vector3 TopLeftPosition() => new( WorldCoordinate.X * WORLD_CELL_SIZE + WORLD_CELL_SIZE, WorldCoordinate.Y * WORLD_CELL_SIZE, WorldHeight() * WORLD_CELL_HEIGHT );
	public Vector3 TopRightPosition() => new( WorldCoordinate.X * WORLD_CELL_SIZE + WORLD_CELL_SIZE, WorldCoordinate.Y * WORLD_CELL_SIZE + WORLD_CELL_SIZE, WorldHeight() * WORLD_CELL_HEIGHT );

	public WorldCell North() => TycoonGame.Instance.WorldManager.GetWorldCell( WorldCoordinate + WorldCoordinate.North );
	public WorldCell South() => TycoonGame.Instance.WorldManager.GetWorldCell( WorldCoordinate + WorldCoordinate.South );
	public WorldCell East() => TycoonGame.Instance.WorldManager.GetWorldCell( WorldCoordinate + WorldCoordinate.East );
	public WorldCell West() => TycoonGame.Instance.WorldManager.GetWorldCell( WorldCoordinate + WorldCoordinate.West );

	public List<WorldCell> AllNonNullNeighbors()
	{
		return new[] { North(), East(), South(), West() }.Where( n => n != null ).ToList();
	}

	public bool HasGroundAsNeighbour()
	{
		return !North().IsWater || !South().IsWater || !East().IsWater || !West().IsWater;
	}

	public bool HasSeaAsNeighbour()
	{
		return North().IsWater || South().IsWater || East().IsWater || West().IsWater;
	}

	public bool IsEdgeOfMap()
	{
		var edgeX = WorldCoordinate.X == 0 || WorldCoordinate.X == TycoonGame.Instance.WorldManager.WorldSize.X - 1;
		var edgey = WorldCoordinate.Y == 0 || WorldCoordinate.Y == TycoonGame.Instance.WorldManager.WorldSize.Y - 1;
		return edgeX || edgey;
	}

	public override string ToString()
	{
		return $"WorldCell ({WorldCoordinate}) (IsWater: {IsWater})";
	}
}
