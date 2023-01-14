using System;
using TycoonGame.World.Data;

namespace TycoonGame.World.Generator;

public class WorldGenerator
{
	private static readonly float WaterPerlinScale = 1f;
	private static readonly float WaterLevel = 0.4f;
	private static readonly float FallOffStrength = 7f; // Bigger is weaker

	public WorldCoordinate WorldSize { get; set; }
	public float Seed { get; set; }

	public WorldCell[,] GenerateProceduralWorld()
	{
		var cells = new WorldCell[WorldSize.X, WorldSize.Y];
		var noiseMap = GenerateWaterNoiseMap();
		var falloffMap = GenerateFallOffMap();

		for ( var y = 0; y < WorldSize.Y; y++ )
		{
			for ( var x = 0; x < WorldSize.X; x++ )
			{
				var noiseValue = noiseMap[x, y] - falloffMap[x, y];

				var cell = new WorldCell();
				cell.IsWater = noiseValue < WaterLevel;
				cell.WorldCoordinate = new WorldCoordinate( x, y );

				cells[x, y] = cell;
			}
		}

		return cells;
	}

	private float[,] GenerateWaterNoiseMap()
	{
		float[,] noiseMap = new float[WorldSize.X, WorldSize.Y];

		for ( int y = 0; y < WorldSize.Y; y++ )
		{
			for ( int x = 0; x < WorldSize.X; x++ )
			{
				noiseMap[x, y] = Sandbox.Utility.Noise.Perlin( x * WaterPerlinScale + Seed, y * WaterPerlinScale + Seed );
			}
		}

		return noiseMap;
	}

	private float[,] GenerateFallOffMap()
	{
		float[,] fallofMap = new float[WorldSize.X, WorldSize.Y];

		for ( int y = 0; y < WorldSize.Y; y++ )
		{
			for ( int x = 0; x < WorldSize.X; x++ )
			{
				float xv = x / (float)WorldSize.X * 2 - 1;
				float yv = y / (float)WorldSize.Y * 2 - 1;
				float v = MathF.Max( MathF.Abs( xv ), MathF.Abs( yv ) );

				fallofMap[x, y] = MathF.Pow( v, 3f ) / (MathF.Pow( v, 3f ) + MathF.Pow( FallOffStrength - FallOffStrength * v, 3f ));
			}
		}

		return fallofMap;
	}
}
