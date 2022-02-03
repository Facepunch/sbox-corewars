using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class GrassBlock : BlockType
	{
		public override string DefaultTexture => "dirt_grass";
		public override string FriendlyName => "Dirt";

		public override byte GetTextureId( BlockFace face, int x, int y, int z )
		{
			if ( face == BlockFace.Top )
				return Map.BlockAtlas.GetTextureId( "grass" );
			else if ( face == BlockFace.Bottom )
				return Map.BlockAtlas.GetTextureId( "dirt" );

			var position = new IntVector3( x, y, z );

			if ( Map.IsAdjacentEmpty( position, (int)BlockFace.Top ) )
				return Map.BlockAtlas.GetTextureId( DefaultTexture );

			return Map.BlockAtlas.GetTextureId( "dirt" );
		}
	}
}
