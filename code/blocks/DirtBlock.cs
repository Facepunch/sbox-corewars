using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class DirtBlock : BlockType
	{
		public override string FriendlyName => "Dirt";
		public override byte TextureId => 0;
		public override byte BlockId => 1;

		public override byte GetTextureId( BlockFace face )
		{
			if ( face == BlockFace.Top )
				return 2;
			else
				return TextureId;
		}
	}
}
