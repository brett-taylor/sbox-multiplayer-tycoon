using Sandbox;

namespace TycoonGame.World;

public partial class WorldCoordinate : BaseNetworkable
{
	public static WorldCoordinate North => new ( 0, 1 );
	public static WorldCoordinate South => new ( 0, -1 );
	public static WorldCoordinate East => new ( 1, 0 );
	public static WorldCoordinate West => new ( -1, 0 );

	[Net]
	public int X { get; set; }

	[Net]
	public int Y { get; set; }

	public WorldCoordinate()
	{
	}

	public WorldCoordinate( int x, int y )
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return $"WorldCoordinate(x={X}, y={Y})";
	}

	public static WorldCoordinate operator +(WorldCoordinate a, WorldCoordinate b)
	{
		return new WorldCoordinate( a.X + b.X, a.Y + b.Y );
	}

	public static WorldCoordinate operator +( WorldCoordinate a, int b )
	{
		return new WorldCoordinate( a.X + b, a.Y + b );
	}

	public static WorldCoordinate operator -( WorldCoordinate a, WorldCoordinate b )
	{
		return new WorldCoordinate( a.X - b.X, a.Y - b.Y );
	}

	public static WorldCoordinate operator -( WorldCoordinate a, int b )
	{
		return new WorldCoordinate( a.X - b, a.Y - b );
	}

	public static WorldCoordinate operator *( WorldCoordinate a, WorldCoordinate b )
	{
		return new WorldCoordinate( a.X * b.X, a.Y * b.Y );
	}

	public static WorldCoordinate operator *( WorldCoordinate a, int b )
	{
		return new WorldCoordinate( a.X * b, a.Y * b );
	}

	public static WorldCoordinate operator /( WorldCoordinate a, WorldCoordinate b )
	{
		return new WorldCoordinate( a.X / b.X, a.Y / b.Y );
	}

	public static WorldCoordinate operator /( WorldCoordinate a, int b )
	{
		return new WorldCoordinate( a.X / b, a.Y / b );
	}
}
