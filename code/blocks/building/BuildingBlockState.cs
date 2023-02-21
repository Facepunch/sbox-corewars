using Facepunch.Voxels;
using Sandbox;
using System;

namespace Facepunch.CoreWars.Blocks
{
	public class BuildingBlockState : BlockState
	{
		public override bool ShouldTick => true;

		public TimeSince LastDamageTime { get; set; }

		public override void Tick()
		{
			if ( Game.IsServer && Health < 100 && LastDamageTime >= 2f )
			{
				Health = (byte)Math.Min( Health + 1, 100 );
				IsDirty = true;
			}

			base.Tick();
		}
	}
}

