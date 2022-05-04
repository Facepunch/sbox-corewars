using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	public class BaseBuildingBlock : BlockType
	{
		public virtual BuildingMaterialType MaterialType => BuildingMaterialType.Unbreakable;
		public virtual float DamageMultiplier => 1f;

		public override BlockState CreateState()
		{
			var state = new BuildingBlockState();
			state.Health = 100;
			return state;
		}
	}
}

