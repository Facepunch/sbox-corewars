using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class CyanPlasticBlock : BaseBuildingBlock
	{
		public override string DefaultTexture => "plastic_cyan_01";
		public override string FriendlyName => "Cyan Plastic";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Plastic;
		public override float DamageMultiplier => 0.8f;
	}
}

