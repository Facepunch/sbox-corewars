using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Utility
{
	public static class BlockTypeExtension
	{
		public static IReadOnlySet<string> GetItemTags( this BlockType self )
		{
			var tags = new HashSet<string>();

			if ( self.IsTranslucent )
				tags.Add( "translucent" );

			if ( self.IsPassable )
				tags.Add( "passablr" );

			return tags;
		}
	}
}
