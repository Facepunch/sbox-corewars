using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class YellowPlasticBlock : BaseBuildingBlock
	{
		public override string DefaultTexture => "plastic_yellow_01";
		public override string FriendlyName => "Yellow Plastic";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Plastic;
		public override float DamageMultiplier => 0.8f;
	}
}

