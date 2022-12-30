using Sandbox;
using TycoonGame.World;

namespace TycoonGame.Building.Core;

[Category( "Simulation" )]
public class BuildingGridDisplay : ModelEntity
{
	private static readonly float HEIGHT = 1f;
	private static readonly Vector2 SIZE_IN_TILE = new Vector2( 9, 9 );

	public override void Spawn()
	{
		base.Spawn();
		Game.AssertClient();

		Transmit = TransmitType.Never;
		Model = GetModel();
	}

	private Model GetModel()
	{
		var actualSizeX = SIZE_IN_TILE.x * WorldManager.TILE_SIZE_IN_UNITS.x;
		var actualSizeY = SIZE_IN_TILE.y * WorldManager.TILE_SIZE_IN_UNITS.y;

		var vb = new VertexBuffer();

		vb.AddCube(
			new Vector3( actualSizeX / 2, actualSizeY / 2, HEIGHT ),
			new Vector3( actualSizeX, actualSizeY, HEIGHT ),
			Rotation
		);

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "materials/ui/blueprint.vmat" );

		return new ModelBuilder().AddMesh( mesh ).Create();
	}

	public void SetTilePosition( Vector2 tilePosition )
	{
		var offset = (SIZE_IN_TILE - Vector2.One) / 2;
		var offsetTilePosition = tilePosition - offset;

		Vector3 position = new Vector3(
			offsetTilePosition.x * WorldManager.TILE_SIZE_IN_UNITS.x,
			offsetTilePosition.y * WorldManager.TILE_SIZE_IN_UNITS.y,
			1f
		);

		Position = position;
	}
}
