using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class BlockType : BaseNetworkable
	{
		public virtual string FriendlyName => "";
		public virtual byte TextureId => 0;
		public virtual byte BlockId => 0;

		public virtual byte GetTextureId( BlockFace face )
		{
			return TextureId;
		}
	}
}
