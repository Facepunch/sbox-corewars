using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class HardenedMetalBlock : BaseBuildingBlock
	{
		public override string DefaultTexture => "hardened_metal_01";
		public override string FriendlyName => "Hardened Metal";
		public override string DestroySound => "break.metal";
		public override string HitSound => "melee.hitmetal";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Metal;
		public override float DamageMultiplier => 0.5f;
	}
}
