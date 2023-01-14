using Sandbox;
using Sandbox.Diagnostics;
using TycoonGame.World.Physical;

namespace TycoonGame.World;

public partial class WorldManager
{
	public WorldGroundEntity WorldGroundEntity { get; private set; }

	public WorldWaterEntity WorldWaterEntity { get; private set; }

	private void CreatePhysicalWorld()
	{
		WorldGroundEntity = CreateWorldGroundEntity();
		WorldWaterEntity = CreateWorldWaterEntity();

		if ( Game.IsClient )
		{
			CreateWorldBedrockEntity();
		}
	}

	private WorldGroundEntity CreateWorldGroundEntity()
	{
		var worldGround = new WorldGroundEntity();
		worldGround.Position = new Vector3( 0f, 0f, 0f );

		if ( Game.IsServer )
		{
			worldGround.Name = "WorldGroundEntity (Server)";
		}
		else
		{
			worldGround.Name = "WorldGroundEntity (Client)";
			worldGround.Owner = Game.LocalPawn;
		}

		return worldGround;
	}

	private WorldWaterEntity CreateWorldWaterEntity()
	{
		var worldWater = new WorldWaterEntity();
		worldWater.Position = new Vector3( 0f, 0f, 0f );

		if ( Game.IsServer )
		{
			worldWater.Name = "WorldWaterEntity (Server)";
		}
		else
		{
			worldWater.Name = "WorldWaterEntity (Client)";
			worldWater.Owner = Game.LocalPawn;
		}

		return worldWater;
	}

	private WorldBedrockEntity CreateWorldBedrockEntity()
	{
		Assert.True( Game.IsClient );

		var worldBedrock = new WorldBedrockEntity();
		worldBedrock.Position = new Vector3( 0f, 0f, 0f );
		worldBedrock.Name = $"WorldBedrockEntity (Client)";
		worldBedrock.Owner = Game.LocalPawn;

		return worldBedrock;
	}
}
