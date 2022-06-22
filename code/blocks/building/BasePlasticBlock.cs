using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	public abstract class BasePlasticBlock : BaseBuildingBlock
	{
		public override string Description => "The cheapest but weakest block.";
		public override string DestroySound => "break.plastic";
		public override string HitSound => "melee.hitplastic";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Plastic;
		public override float DamageMultiplier => 0.8f;
	}
}

