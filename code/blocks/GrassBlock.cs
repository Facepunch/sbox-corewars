using Facepunch.Voxels;
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
			var position = new IntVector3( x, y, z );
			var sunlightLevel = VoxelWorld.GetSunLight( chunk.Offset + position + Chunk.BlockDirections[0] ) ;

			if ( sunlightLevel < 5 )
				return VoxelWorld.BlockAtlas.GetTextureId( "dirt" );

			if ( face == BlockFace.Top )
				return VoxelWorld.BlockAtlas.GetTextureId( "grass" );
			else if ( face == BlockFace.Bottom )
				return VoxelWorld.BlockAtlas.GetTextureId( "dirt" );

			var adjacentBlockId = VoxelWorld.GetAdjacentBlock( chunk.Offset + position, (int)BlockFace.Top );
			var adjacentBlock = VoxelWorld.GetBlockType( adjacentBlockId );

			if ( adjacentBlock.IsTranslucent )
				return VoxelWorld.BlockAtlas.GetTextureId( DefaultTexture );

			return VoxelWorld.BlockAtlas.GetTextureId( "dirt" );
		}
	}
}
