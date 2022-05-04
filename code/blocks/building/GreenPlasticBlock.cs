using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class GreenPlasticBlock : BaseBuildingBlock
	{
		public override string DefaultTexture => "plastic_green_01";
		public override string FriendlyName => "Green Plastic";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Plastic;
		public override float DamageMultiplier => 0.8f;
	}
}

