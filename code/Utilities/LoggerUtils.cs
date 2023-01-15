using Sandbox;
using Sandbox.Diagnostics;
using System;

namespace TycoonGame.Utilities;

public static class LoggerUtils
{
	private static Logger LOGGER = CreateLogger( typeof(LoggerUtils) );

	public static Logger CreateLogger( Type type )
	{
		var serverOrClient = Game.IsServer ? "Server" : "Client";
		return new Logger( $"{type.Name}/{serverOrClient}" );
	}

	public static void QuickLog( object log )
	{
		if ( log == null )
		{
			LOGGER.Info( "Null" );
			return;
		}
		
		LOGGER.Info( log.ToString() );
	}
}
