using Sandbox;
using TycoonGame.Utilities.Enumertion;
using TycoonGame.Utilities.Extensions;
using TycoonGame.World.Data;

namespace TycoonGame.World.Physical;

[Category( "World" )]
public class WorldGroundEntity : ModelEntity
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Never;

		Tags.Add( CustomTags.Solid );
		Tags.Add( CustomTags.World );
		Tags.Add( CustomTags.Ground );

		Model = new ModelBuilder()
			.AddMesh( CreateGroundMesh() )
			.AddMesh( CreateGroundEdgeMesh() )
			.Create();

		SetupPhysics();
	}

	private Mesh CreateGroundMesh()
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		for ( var y = 0; y < TycoonGame.Instance.WorldManager.WorldSize.Y; y++ )
		{
			for ( var x = 0; x < TycoonGame.Instance.WorldManager.WorldSize.X; x++ )
			{
				var worldCell = TycoonGame.Instance.WorldManager.WorldCells[x, y];
				if ( worldCell.IsWater )
					continue;

				var ray = new Ray( worldCell.CenterTilePosition(), Rotation.Up );
				var width = Rotation.Forward * WorldCell.WORLD_CELL_SIZE * 0.5f;
				var height = Rotation.Left * WorldCell.WORLD_CELL_SIZE * 0.5f;

				vb.AddQuad( ray, width, height );
			}
		}

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "materials/world/grass.vmat" );

		return mesh;
	}

	private Mesh CreateGroundEdgeMesh()
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		for ( var y = 0; y < TycoonGame.Instance.WorldManager.WorldSize.Y; y++ )
		{
			for ( var x = 0; x < TycoonGame.Instance.WorldManager.WorldSize.X; x++ )
			{
				var worldCell = TycoonGame.Instance.WorldManager.WorldCells[x, y];
				if ( worldCell.IsWater )
					continue;

				var northTile = worldCell.North();
				if ( northTile != null && northTile.IsWater )
				{
					var centerPoint = new Vector3( worldCell.BottomLeftPosition().x + WorldCell.WORLD_CELL_SIZE / 2, worldCell.BottomLeftPosition().y + WorldCell.WORLD_CELL_SIZE, 0 );
					var rotation = Rotation.FromYaw( 90f );
					vb.CreateFaceingQuad( centerPoint, rotation, WorldCell.WORLD_CELL_SIZE, WorldCell.WORLD_CELL_HEIGHT );
				}

				var southTile = worldCell.South();
				if ( southTile != null && southTile.IsWater )
				{
					var centerPoint = new Vector3( worldCell.BottomLeftPosition().x + WorldCell.WORLD_CELL_SIZE / 2, worldCell.BottomLeftPosition().y, 0 );
					var rotation = Rotation.FromYaw( -90f );
					vb.CreateFaceingQuad( centerPoint, rotation, WorldCell.WORLD_CELL_SIZE, WorldCell.WORLD_CELL_HEIGHT );
				}

				var westTile = worldCell.West();
				if ( westTile != null && westTile.IsWater )
				{
					var centerPoint = new Vector3( worldCell.BottomLeftPosition().x, worldCell.BottomLeftPosition().y + WorldCell.WORLD_CELL_SIZE / 2, 0 );
					var rotation = Rotation.FromYaw( 180f );
					vb.CreateFaceingQuad( centerPoint, rotation, WorldCell.WORLD_CELL_SIZE, WorldCell.WORLD_CELL_HEIGHT );
				}

				var eastTile = worldCell.East();
				if ( eastTile != null && eastTile.IsWater )
				{
					var centerPoint = new Vector3( worldCell.BottomLeftPosition().x + WorldCell.WORLD_CELL_SIZE, worldCell.BottomLeftPosition().y + WorldCell.WORLD_CELL_SIZE / 2, 0 );
					var rotation = Rotation.FromYaw( 0f );
					vb.CreateFaceingQuad( centerPoint, rotation, WorldCell.WORLD_CELL_SIZE, WorldCell.WORLD_CELL_HEIGHT );
				}
			}
		}

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "materials/world/grass_edge.vmat" );

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
				if ( worldCell.IsWater )
					continue;

				if (worldCell.HasSeaAsNeighbour())
				{
					var extend = new Vector3( WorldCell.WORLD_CELL_SIZE / 2f, WorldCell.WORLD_CELL_SIZE / 2f, WorldCell.WORLD_CELL_HEIGHT / 2f );
					var centerPosition = new Vector3(worldCell.CenterTilePosition(), WorldCell.WORLD_CELL_HEIGHT / 2f);
					PhysicsBody.AddBoxShape( centerPosition, Rotation, extend, false );
				}
				else
				{
					var extend = new Vector3( WorldCell.WORLD_CELL_SIZE / 2f, WorldCell.WORLD_CELL_SIZE / 2f, 1f );
					PhysicsBody.AddBoxShape( worldCell.CenterTilePosition(), Rotation, extend, false );
				}
			}
		}
	}
}
