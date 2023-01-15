using Sandbox;
using System.Collections.Generic;
using System.IO;
using TycoonGame.Building.Archetypes;
using TycoonGame.Building.Definitions;

namespace TycoonGame.World;

public partial class WorldManager
{
	private Dictionary<WorldCoordinate, BaseBuilding> Buildings { get; set; } = new Dictionary<WorldCoordinate, BaseBuilding>();

	public void RegisterNewBuilding( WorldCoordinate worldCoordinate, BaseBuilding building )
	{
		if (Buildings.ContainsKey( worldCoordinate ) ) 
		{
			throw new System.Exception( $"Building already found on {worldCoordinate}" );
		}

		Buildings.Add( worldCoordinate, building );
		RegisterNewBuildingsClient( To.Everyone, SerializeBuildingDictionary() );
	}

	public void RegisterNewBuildings( Dictionary<WorldCoordinate, BaseBuilding> newBuildings)
	{
		foreach(var kvp in newBuildings )
		{
			if ( Buildings.ContainsKey( kvp.Key ) )
			{
				throw new System.Exception( $"Building already found on {kvp.Key}" );
			}

			Buildings.Add( kvp.Key, kvp.Value);
		}

		RegisterNewBuildingsClient( To.Everyone, SerializeBuildingDictionary() );
	}

	public void RefreshBuildingsForNewClient( IClient client )
	{
		RegisterNewBuildingsClient( To.Single( client ), SerializeBuildingDictionary() );
	}

	public bool DoesBuildingExistOn( WorldCoordinate worldCoordinate )
	{
		return Buildings.ContainsKey( worldCoordinate );
	}

	public bool DoesBuildingExistOn( WorldCoordinate worldCoordinate, out BaseBuilding building )
	{
		if (Buildings.ContainsKey( worldCoordinate) )
		{
			building = Buildings[worldCoordinate];
			return true;
		}
		else
		{
			building = null;
			return false;
		}
	}

	public bool DoesBuildingTypeExistOn( WorldCoordinate worldCoordinate, BuildingDefinition buildingDefinition )
	{
		return DoesBuildingExistOn( worldCoordinate, out BaseBuilding baseBuilding ) && baseBuilding.BuildingDefinition == buildingDefinition;
	}

	[ClientRpc]
	private void RegisterNewBuildingsClient( byte[] update )
	{
		Buildings = DeserializeBuildingDictionary( update );
	}

	private byte[] SerializeBuildingDictionary()
	{
		using var s = new MemoryStream();
		using var w = new BinaryWriter( s );

		Buildings.Count.Write( w );
		
		foreach (var kvp in Buildings)
		{
			kvp.Key.X.Write( w );
			kvp.Key.Y.Write( w );
			kvp.Value.NetworkIdent.Write( w );
		}

		return s.ToArray();
	}

	private Dictionary<WorldCoordinate, BaseBuilding> DeserializeBuildingDictionary( byte[] bytes)
	{
		var dictionary = new Dictionary<WorldCoordinate, BaseBuilding>();

		using var s = new MemoryStream( bytes );
		using var r = new BinaryReader( s );
		var size = r.ReadInt32();

		for (int i = 0; i < size; i++ )
		{
			var worldCoordinate = new WorldCoordinate( r.ReadInt32(), r.ReadInt32() );
			var building = (BaseBuilding) FindByIndex( r.ReadInt32() );
			dictionary.Add( worldCoordinate, building );
		}

		return dictionary;
	}

}
