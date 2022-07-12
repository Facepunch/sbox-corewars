using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Utility
{
	public static class BlockTypeExtension
	{
		public static ItemTag[] GetItemTags( this BlockType self )
		{
			var tags = new List<ItemTag>();

			if ( self.IsTranslucent )
				tags.Add( new ItemTag( "Translucent", Color.Cyan ) );

			if ( self.IsPassable )
				tags.Add( new ItemTag( "Passable", Color.Green ) );

			return tags.ToArray();
		}
	}
}
