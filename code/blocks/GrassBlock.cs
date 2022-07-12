using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class GrassBlock : BlockType
	{
		public override string DefaultTexture => "grass_dirt_01";
		public override string FriendlyName => "Grass";
		public override int MinHueShift => 0;
		public override int MaxHueShift => 6;

		public override byte GetTextureId( BlockFace face, Chunk chunk, int x, int y, int z )
		{
			var position = new IntVector3( x, y, z );
			var sunlightLevel = 15; //World.GetSunLight( chunk.Offset + position + Chunk.BlockDirections[0] ) ;

			if ( sunlightLevel < 5 )
				return World.BlockAtlas.GetTextureId( "dirt_02" );

			if ( face == BlockFace.Top )
				return World.BlockAtlas.GetTextureId( "grass_01" );
			else if ( face == BlockFace.Bottom )
				return World.BlockAtlas.GetTextureId( "dirt_02" );

			var adjacentBlockId = World.GetAdjacentBlock( chunk.Offset + position, (int)BlockFace.Top );
			var adjacentBlock = World.GetBlockType( adjacentBlockId );

			if ( adjacentBlock.IsTranslucent )
				return World.BlockAtlas.GetTextureId( DefaultTexture );

			return World.BlockAtlas.GetTextureId( "dirt_02" );
		}
	}
}
