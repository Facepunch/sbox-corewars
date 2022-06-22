using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	public abstract class BaseBuildingBlock : BlockType
	{
		public virtual BuildingMaterialType MaterialType => BuildingMaterialType.Unbreakable;
		public virtual string DestroySound => null;
		public virtual string HitSound => null;
		public virtual float DamageMultiplier => 1f;

		public override BlockState CreateState()
		{
			return new BuildingBlockState();
		}
	}
}

