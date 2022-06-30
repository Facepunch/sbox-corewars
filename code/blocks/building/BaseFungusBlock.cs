using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	public abstract class BaseFungusBlock : BaseBuildingBlock
	{
		public override string Description => "The cheapest but weakest block.";
		public override string DestroySound => "break.fungus";
		public override string HitSound => "melee.hitfungus";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Fungus;
		public override float DamageMultiplier => 0.8f;
		public override string FriendlyName => "Fungus";
	}
}

