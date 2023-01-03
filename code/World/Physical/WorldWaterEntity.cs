using Sandbox;
using TycoonGame.Utilities.Enumertion;

namespace TycoonGame.World.Physical;

[Category( "World" )]
public class WorldWaterEntity : ModelEntity
{
	public Material WaterMaterial { get; private set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Never;

		Tags.Add( CustomTags.World );
		Tags.Add( CustomTags.Water );

		WaterMaterial = Material.Load( "materials/world/water.vmat" );

		Model = new ModelBuilder()
			.AddMesh( CreateWaterMesh() )
			.Create();

		SetupPhysics();		
	}

	private Mesh CreateWaterMesh()
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		for ( var y = 0; y < TycoonGame.Instance.WorldManager.WorldSize.Y; y++ )
		{
			for ( var x = 0; x < TycoonGame.Instance.WorldManager.WorldSize.X; x++ )
			{
				var worldCell = TycoonGame.Instance.WorldManager.WorldCells[x, y];

				// TODO Do we want to create water under the ground or not. Do it rn to test depth
				/*if ( !worldCell.IsWater )
					continue;*/

				var centerPoint = new Vector3( worldCell.CenterTilePosition(), 0f );
				var ray = new Ray( centerPoint, Rotation.Up );
				var width = Rotation.Forward * WorldCell.WORLD_CELL_SIZE * 0.5f;
				var height = Rotation.Left * WorldCell.WORLD_CELL_SIZE * 0.5f;

				vb.AddQuad( ray, width, height );
			}
		}

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = WaterMaterial;

		return mesh;
	}

	private void SetupPhysics()
	{
		SetupPhysicsFromSphere( PhysicsMotionType.Static, Vector3.Zero, 0f );

		for ( var y = 0; y < TycoonGame.Instance.WorldManager.WorldSize.Y; y++ )
		{
			for ( var x = 0; x < TycoonGame.Instance.WorldManager.WorldSize.X; x++ )
			{
				var worldCell = TycoonGame.Instance.WorldManager.WorldCells[x, y];
				if ( !worldCell.IsWater )
					continue;

				var extend = new Vector3( WorldCell.WORLD_CELL_SIZE / 2f, WorldCell.WORLD_CELL_SIZE / 2f, 1f );
				PhysicsBody.AddBoxShape( worldCell.CenterTilePosition(), Rotation, extend, false );
			}
		}
	}
}
