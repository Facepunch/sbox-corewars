using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WoodBlock : BlockType
	{
		public override string FriendlyName => "Wood";
		public override byte TextureId => 6;
		public override byte BlockId => 5;
		public override byte GetTextureId( BlockFace face )
		{
			if ( face == BlockFace.Top )
				return 7;
			else if ( face == BlockFace.Bottom )
				return 7;
			else
				return TextureId;
		}
	}
}

