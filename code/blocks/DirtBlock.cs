using Facepunch.CoreWars.Voxel;

namespace Facepunch.CoreWars.Blocks
{
	public class DirtBlock : BlockType
	{
		public override byte TextureId => 0;
		public override byte BlockId => 1;

		public override byte GetTextureId( BlockFace face )
		{
			if ( face == BlockFace.Top )
				return (byte)(TextureId + 1);
			else
				return TextureId;
		}
	}
}
