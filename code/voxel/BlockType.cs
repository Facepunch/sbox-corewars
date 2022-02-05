using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Voxel
{
	public class BlockType
	{
		public Map Map { get; init; }

		public virtual string DefaultTexture => "";
		public virtual string FriendlyName => "";
		public virtual bool AttenuatesSunLight => false;
		public virtual bool HasTexture => true;
		public virtual bool IsPassable => false;
		public virtual bool IsTranslucent => false;
		public virtual IntVector3 LightLevel => 0;
		public virtual Vector3 LightFilter => Vector3.One;
		public virtual string ServerEntity => "";
		public virtual string ClientEntity => "";

		public virtual byte GetTextureId( BlockFace face, Chunk chunk, int x, int y, int z )
		{
			if ( string.IsNullOrEmpty( DefaultTexture ) ) return 0;

			return Map.BlockAtlas.GetTextureId( DefaultTexture );
		}

		public virtual void OnBlockAdded( Chunk chunk, int x, int y, int z )
		{
			
		}

		public virtual void OnBlockRemoved( Chunk chunk, int x, int y, int z )
		{

		}

		public BlockType()
		{
			Map = Map.Current;
		}
	}
}
