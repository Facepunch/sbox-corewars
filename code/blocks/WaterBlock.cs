using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WaterBlock : BlockType
	{
		public override string DefaultTexture => "stone";
		public override string FriendlyName => "Water";
		public override bool IsTranslucent => true;
		public override bool IsPassable => true;
		public override bool IsLiquid => true;

		public override bool ShouldCullFace( BlockFace face, BlockType neighbour )
		{
			if ( neighbour == this )
				return true;

			return false;
		}
	}
}
