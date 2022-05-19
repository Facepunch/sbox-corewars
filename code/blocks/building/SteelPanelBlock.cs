using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class SteelPanelBlock : BaseBuildingBlock
	{
		public override string DefaultTexture => "steel_panel_01";
		public override string FriendlyName => "Steel Panel";
		public override string HitSound => "melee.hitmetal";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Metal;
		public override float DamageMultiplier => 0.25f;
	}
}
