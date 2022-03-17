using Facepunch.Voxels;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	public class AreaBlocksAction : EditorAction
	{
		public override string Name => "Area Blocks";
		 
		private IEnumerable<IntVector3> Positions { get; set; }
		private BlockState[] OldBlockStates { get; set; }
		private byte[] OldBlockIds { get; set; }
		private byte BlockId { get; set; }
		private IntVector3 Mins { get; set; }
		private IntVector3 Maxs { get; set; }

		public void Initialize( IntVector3 mins, IntVector3 maxs, byte blockId )
		{
			var world = VoxelWorld.Current;

			Mins = mins;
			Maxs = maxs;
			BlockId = blockId;
			Positions = world.GetPositionsInBox( Mins, Maxs );

			var totalBlocks = Positions.Count();
			var currentIndex = 0;

			OldBlockStates = new BlockState[totalBlocks];
			OldBlockIds = new byte[totalBlocks];

			foreach ( var position in Positions )
			{
				OldBlockStates[currentIndex] = world.GetState<BlockState>( position );
				OldBlockIds[currentIndex] = world.GetBlock( position );
				currentIndex++;
			}
		}

		public override void Perform()
		{
			var world = VoxelWorld.Current;

			foreach ( var position in Positions )
			{
				world.SetBlockOnServer( position, BlockId );
			}

			base.Perform();
		}

		public override void Undo()
		{
			var world = VoxelWorld.Current;
			var currentIndex = 0;

			foreach ( var position in Positions )
			{
				world.SetBlockOnServer( position, OldBlockIds[currentIndex] );
				world.SetState( position, OldBlockStates[currentIndex] );
				currentIndex++;
			}

			base.Undo();
		}
	}
}
