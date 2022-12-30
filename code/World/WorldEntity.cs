using Sandbox;
using TycoonGame.Utilities.Enumertion;

namespace TycoonGame.World;

[Category( "Simulation" )]
public partial class WorldEntity : ModelEntity
{
	private static readonly float GRASS_HEIGHT = 100f;
	private static readonly float DIRT_WIDTH = 20f;
	private static readonly float DIRT_HEIGHT = 1000f;

	public override void Spawn()
	{
		base.Spawn();
		
		Transmit = TransmitType.Always;

		var worldSize = WorldManager.GetWorldSizeInUnits();
		SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( 0f, 0f, -GRASS_HEIGHT ), new Vector3( worldSize.x, worldSize.y, 0f ) );

		Tags.Add( CustomTags.Solid );
		Tags.Add( CustomTags.World );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		var worldSize = WorldManager.GetWorldSizeInUnits();

		Model = new ModelBuilder()
			.AddMesh( GetGrassMesh() )
			.AddMesh( GetDirtMesh(
				new Vector3( worldSize.x / 2, DIRT_WIDTH / 2, (-DIRT_HEIGHT / 2) - GRASS_HEIGHT ),
				new Vector3( worldSize.x, DIRT_WIDTH, DIRT_HEIGHT ) )
			) // South wall
			.AddMesh( GetDirtMesh(
				new Vector3( DIRT_WIDTH / 2, worldSize.y / 2, (-DIRT_HEIGHT / 2) - GRASS_HEIGHT ),
				new Vector3( DIRT_WIDTH, worldSize.y, DIRT_HEIGHT ) )
			) // West wall
			.AddMesh( GetDirtMesh(
				new Vector3( worldSize.x / 2, worldSize.y - (DIRT_WIDTH / 2), (-DIRT_HEIGHT / 2) - GRASS_HEIGHT ),
				new Vector3( worldSize.x, DIRT_WIDTH, DIRT_HEIGHT ) )
			) // North wall
			.AddMesh( GetDirtMesh(
				new Vector3( worldSize.x - (DIRT_WIDTH / 2), worldSize.y / 2, (-DIRT_HEIGHT / 2) - GRASS_HEIGHT ),
				new Vector3( DIRT_WIDTH, worldSize.y, DIRT_HEIGHT ) )
			) // East wall
			.Create();
	}

	public void Create()
	{
		var worldSize = WorldManager.GetWorldSizeInUnits();
		SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( 0f, 0f, -GRASS_HEIGHT ), new Vector3( worldSize.x, worldSize.y, 0f ) );
	}

	private Mesh GetGrassMesh()
	{
		var worldSize = WorldManager.GetWorldSizeInUnits();

		var vb = new VertexBuffer();
		vb.AddCube(
			new Vector3( worldSize.x/ 2, worldSize.y / 2, (-GRASS_HEIGHT / 2)),
			new Vector3( worldSize.x, worldSize.y, GRASS_HEIGHT ),
			Rotation
		);

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "materials/world/grass.vmat" );

		return mesh;
	}

	private Mesh GetDirtMesh(Vector3 center, Vector3 size)
	{
		var vb = new VertexBuffer();
		vb.AddCube(center, size, Rotation);

		var mesh = new Mesh();
		mesh.CreateBuffers( vb );
		mesh.Material = Material.Load( "materials/world/dirt.vmat" );

		return mesh;
	}

}
