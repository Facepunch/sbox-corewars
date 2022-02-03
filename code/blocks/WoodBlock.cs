using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WoodBlock : BlockType
	{
		public override string DefaultTexture => "log_side";
		public override string FriendlyName => "Wood";

		public override byte GetTextureId( BlockFace face, int x, int y, int z )
		{
			if ( face == BlockFace.Top || face == BlockFace.Bottom )
				return Map.BlockAtlas.GetTextureId( "log_top" );

			return base.GetTextureId( face, x, y, z );
		}
	}
}

