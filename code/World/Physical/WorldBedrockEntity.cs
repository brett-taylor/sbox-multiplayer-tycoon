using Sandbox;
using TycoonGame.Utilities.Extensions;
using TycoonGame.World.Data;

namespace TycoonGame.World.Physical;

[Category( "World" )]
public class WorldBedrockEntity : ModelEntity
{
	private static readonly float WATER_EDGE_HEIGHT = 100f;
	private static readonly float BEDROCK_HEIGHT = 700f;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Never;

		Model = new ModelBuilder()
			.AddMesh( CreateBedrockEdgeBorder() )
			.AddMesh( CreateBedrockMesh() )
			.Create();
	}

	private Mesh CreateBedrockEdgeBorder()
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		for ( var y = 0; y < TycoonGame.Instance.WorldManager.WorldSize.Y; y++ )
		{
			for ( var x = 0; x < TycoonGame.Instance.WorldManager.WorldSize.X; x++ )
			{
				var worldCell = TycoonGame.Instance.WorldManager.WorldCells[x, y];
				if ( !worldCell.IsEdgeOfMap() )
					continue;

				var northTile = worldCell.North();
				if ( northTile == null )
				{
					var centerPoint = new Vector3( worldCell.BottomLeftPosition().x + WorldCell.WORLD_CELL_SIZE / 2, worldCell.BottomLeftPosition().y + WorldCell.WORLD_CELL_SIZE, -WATER_EDGE_HEIGHT );
					var rotation = Rotation.FromYaw( 90f );
					vb.CreateFaceingQuad( centerPoint, rotation, WorldCell.WORLD_CELL_SIZE, WATER_EDGE_HEIGHT );
				}

				var southTile = worldCell.South();
				if ( southTile == null )
				{
					var centerPoint = new Vector3( worldCell.BottomLeftPosition().x + WorldCell.WORLD_CELL_SIZE / 2, worldCell.BottomLeftPosition().y, -WATER_EDGE_HEIGHT );
					var rotation = Rotation.FromYaw( -90f );
					vb.CreateFaceingQuad( centerPoint, rotation, WorldCell.WORLD_CELL_SIZE, WATER_EDGE_HEIGHT );
				}

				var westTile = worldCell.West();
				if ( westTile == null )
				{
					var centerPoint = new Vector3( worldCell.BottomLeftPosition().x, worldCell.BottomLeftPosition().y + WorldCell.WORLD_CELL_SIZE / 2, -WATER_EDGE_HEIGHT );
					var rotation = Rotation.FromYaw( 180f );
					vb.CreateFaceingQuad( centerPoint, rotation, WorldCell.WORLD_CELL_SIZE, WATER_EDGE_HEIGHT );
				}

				var eastTile = worldCell.East();
				if ( eastTile == null )
				{
					var centerPoint = new Vector3( worldCell.BottomLeftPosition().x + WorldCell.WORLD_CELL_SIZE, worldCell.BottomLeftPosition().y + WorldCell.WORLD_CELL_SIZE / 2, -WATER_EDGE_HEIGHT );
					var rotation = Rotation.FromYaw( 0f );
					vb.CreateFaceingQuad( centerPoint, rotation, WorldCell.WORLD_CELL_SIZE, WATER_EDGE_HEIGHT );
				}
			}
		}

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "materials/world/water_edge.vmat" );

		return mesh;
	}

	private Mesh CreateBedrockMesh()
	{
		var vb = new VertexBuffer();
		vb.Init( true );

		var zPosition = -WATER_EDGE_HEIGHT * 2 + -BEDROCK_HEIGHT;
		var northSouthFaceWidth = TycoonGame.Instance.WorldManager.WorldSize.X * WorldCell.WORLD_CELL_SIZE;
		var northSouthFaceMidPoint = TycoonGame.Instance.WorldManager.WorldSize.X / 2 * WorldCell.WORLD_CELL_SIZE;
		var eastWestFaceWidth = TycoonGame.Instance.WorldManager.WorldSize.Y * WorldCell.WORLD_CELL_SIZE;
		var eastWestFaceMidPoint = TycoonGame.Instance.WorldManager.WorldSize.Y / 2 * WorldCell.WORLD_CELL_SIZE;

		var northFaceCenterPoint = new Vector3( northSouthFaceMidPoint, TycoonGame.Instance.WorldManager.WorldSize.Y * WorldCell.WORLD_CELL_SIZE, zPosition );
		var northFaceRotation = Rotation.FromYaw( 90f );
		vb.CreateFaceingQuad( northFaceCenterPoint, northFaceRotation, northSouthFaceWidth, BEDROCK_HEIGHT );

		var southFaceCenterPoint = new Vector3( northSouthFaceMidPoint, 0, zPosition );
		var southFaceRotation = Rotation.FromYaw( -90f );
		vb.CreateFaceingQuad( southFaceCenterPoint, southFaceRotation, northSouthFaceWidth, BEDROCK_HEIGHT );

		var westFaceCenterPoint = new Vector3( 0, eastWestFaceMidPoint, zPosition );
		var westFaceRotation = Rotation.FromYaw( 180f );
		vb.CreateFaceingQuad( westFaceCenterPoint, westFaceRotation, eastWestFaceWidth, BEDROCK_HEIGHT );

		var eastFaceCenterPoint = new Vector3( TycoonGame.Instance.WorldManager.WorldSize.X * WorldCell.WORLD_CELL_SIZE, eastWestFaceMidPoint, zPosition );
		var eastFaceRotation = Rotation.FromYaw( 0f );
		vb.CreateFaceingQuad( eastFaceCenterPoint, eastFaceRotation, eastWestFaceWidth, BEDROCK_HEIGHT );

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "materials/world/bedrock.vmat" );

		return mesh;
	}
}
