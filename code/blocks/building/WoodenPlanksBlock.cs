using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class WoodenPlanksBlock : BaseBuildingBlock
	{
		public override string DefaultTexture => "planks_01";
		public override string FriendlyName => "Wooden Planks";
		public override string Description => "A slightly stronger defensive block.";
		public override string DestroySound => "break.wood";
		public override string HitSound => "melee.hitwood";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Wooden;
		public override float DamageMultiplier => 0.8f;
	}
}
