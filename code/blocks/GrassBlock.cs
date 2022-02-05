﻿using Facepunch.CoreWars.Voxel;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class GrassBlock : BlockType
	{
		public override string DefaultTexture => "dirt_grass";
		public override string FriendlyName => "Dirt";

		public override byte GetTextureId( BlockFace face, Chunk chunk, int x, int y, int z )
		{
			if ( face == BlockFace.Top )
				return Map.BlockAtlas.GetTextureId( "grass" );
			else if ( face == BlockFace.Bottom )
				return Map.BlockAtlas.GetTextureId( "dirt" );

			var position = chunk.Offset + new IntVector3( x, y, z );
			var adjacentBlockId = Map.GetAdjacentBlock( position, (int)BlockFace.Top );
			var adjacentBlock = Map.GetBlockType( adjacentBlockId );

			if ( adjacentBlock.IsTranslucent )
				return Map.BlockAtlas.GetTextureId( DefaultTexture );

			return Map.BlockAtlas.GetTextureId( "dirt" );
		}
	}
}
