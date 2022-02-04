using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WoodbirchBlock : BlockType
	{
		public override string DefaultTexture => "birchlog_side";
		public override string FriendlyName => "Birch Wood";

		public override byte GetTextureId( BlockFace face, Chunk chunk, int x, int y, int z )
		{
			if ( face == BlockFace.Top || face == BlockFace.Bottom )
				return Map.BlockAtlas.GetTextureId( "birchlog_top" );

			return base.GetTextureId( face, chunk, x, y, z );
		}
	}
}

