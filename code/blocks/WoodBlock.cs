using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WoodBlock : BlockType
	{
		public override string DefaultTexture => "log_side";
		public override string FriendlyName => "Wood";

		public override byte GetTextureId( BlockFace face, Chunk chunk, int x, int y, int z )
		{
			if ( face == BlockFace.Top || face == BlockFace.Bottom )
				return World.BlockAtlas.GetTextureId( "log_top" );

			return base.GetTextureId( face, chunk, x, y, z );
		}
	}
}

