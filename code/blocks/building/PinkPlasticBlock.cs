using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class PinkPlasticBlock : BaseBuildingBlock
	{
		public override string DefaultTexture => "plastic_pink_01";
		public override string FriendlyName => "Pink Plastic";
		public override BuildingMaterialType MaterialType => BuildingMaterialType.Plastic;
		public override float DamageMultiplier => 0.8f;
	}
}

