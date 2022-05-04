using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	public class BuildingBlockState : BlockState
	{
		public override bool ShouldTick => true;

		public TimeSince LastDamageTime { get; set; }

		public override void Tick()
		{
			if ( Health < 100 && LastDamageTime >= 2f )
			{
				Health = 100;
				IsDirty = true;
			}

			base.Tick();
		}
	}
}

