using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class TemperedGlassBlock : BaseBuildingBlock
	{
		public override string DefaultTexture => "tempered_glass_01";
		public override string FriendlyName => "Tempered Glass";
		public override string DestroySound => "break.glass";
		public override string HitSound => "melee.hitglass";
		public override bool IsTranslucent => true;
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Blastproof;
		public override float DamageMultiplier => 0.8f;
	}
}
