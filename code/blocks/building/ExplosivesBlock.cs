using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class ExplosivesBlock : BaseBuildingBlock
	{
		public override string Icon => "textures/items/bomb.png";
		public override string FriendlyName => "Explosives";
		public override string ServerEntity => "cw_explosives";
		public override bool IsTranslucent => true;
		public override bool HasTexture => false;
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Explosives;

		public override bool ShouldCullFace( BlockFace face, BlockType neighbour )
		{
			return false;
		}
	}
}
