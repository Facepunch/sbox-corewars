using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class BlockType
	{
		public virtual string FriendlyName => "";
		public virtual bool IsTranslucent => false;
		public virtual byte TextureId => 0;
		public virtual byte BlockId => 0;

		public virtual byte GetTextureId( BlockFace face )
		{
			return TextureId;
		}
	}
}
