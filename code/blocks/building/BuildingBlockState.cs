using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Blocks
{
	public class BuildingBlockState : BlockState
	{
		public override bool ShouldTick => true;

		public TimeSince LastDamageTime { get; set; }

		public override void Tick( IntVector3 position )
		{
			if ( LastDamageTime >= 2f )
			{
				Health = 100;
			}

			base.Tick( position );
		}
	}
}

