using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class BlockType
	{
		public Map Map { get; init; }

		public virtual string DefaultTexture => "";
		public virtual string FriendlyName => "";
		public virtual bool IsTranslucent => false;
		public virtual byte LightLevel => 0;

		public virtual byte GetTextureId( BlockFace face, int x, int y, int z )
		{
			if ( string.IsNullOrEmpty( DefaultTexture ) ) return 0;

			return Map.BlockAtlas.GetTextureId( DefaultTexture );
		}

		public BlockType()
		{
			Map = Map.Current;
		}
	}
}
