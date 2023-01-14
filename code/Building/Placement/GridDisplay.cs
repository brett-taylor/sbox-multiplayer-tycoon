using Sandbox;
using TycoonGame.World;
using TycoonGame.World.Data;

namespace TycoonGame.Building.Placement;

[Category( "Simulation" )]
public class GridDisplay : ModelEntity
{
	private static readonly float HEIGHT = 1f;
	private static readonly float HEIGHT_OFFSET = 1f;
	private static readonly WorldCoordinate SIZE_IN_CELLS = new WorldCoordinate( 9, 9 );

	public override void Spawn()
	{
		base.Spawn();
		Game.AssertClient();

		Transmit = TransmitType.Never;
		EnableLagCompensation = false;
		Predictable = false;

		Model = GetModel();
	}

	private Model GetModel()
	{
		var actualSizeX = SIZE_IN_CELLS.X * WorldCell.WORLD_CELL_SIZE;
		var actualSizeY = SIZE_IN_CELLS.Y * WorldCell.WORLD_CELL_SIZE;

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

	public void SetPosition( WorldCell worldCell )
	{
		var offset = (SIZE_IN_CELLS - 1) / 2;
		var offsetTilePosition = worldCell.WorldCoordinate - offset;

		var height = worldCell.WorldHeight() * WorldCell.WORLD_CELL_HEIGHT + HEIGHT_OFFSET;

		Vector3 position = new Vector3(
			offsetTilePosition.X * WorldCell.WORLD_CELL_SIZE,
			offsetTilePosition.Y * WorldCell.WORLD_CELL_SIZE,
			height
		);

		Position = position;
	}
}

