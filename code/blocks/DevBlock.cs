using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	[Library]
	public class DevBlock : BlockType
	{
		public override string DefaultTexture => "dev_grid";
		public override string FriendlyName => "DevBlock";
		public override float DetailSpawnChance => 0.05f;
		public override string[] DetailModels => new string[]
		{
			"models/rust_nature/overgrowth/patch_grass_small.vmdl",
			"models/rust_nature/overgrowth/patch_grass_tall.vmdl",
			"models/rust_nature/overgrowth/cracks_grass_b.vmdl"
		};

		protected override void OnSpawnDetailModel( ModelEntity entity )
		{
			entity.Scale = 0.5f;
		}
	}
}
