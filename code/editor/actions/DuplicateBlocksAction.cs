using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	public class DuplicateBlocksAction : EditorAction
	{
		public override string Name => "Duplicate Blocks";
		 
		private IEnumerable<IntVector3> SourcePositions { get; set; }
		private BlockState[] NewBlockStates { get; set; }
		private byte[] NewBlockIds { get; set; }
		private BlockState[] OldBlockStates { get; set; }
		private byte[] OldBlockIds { get; set; }
		private IntVector3[] TargetPositions { get; set; }
		private IntVector3 SourceMins { get; set; }
		private IntVector3 SourceMaxs { get; set; }
		private IntVector3 TargetMins { get; set; }

		public void Initialize( IntVector3 sourceMins, IntVector3 sourceMaxs, IntVector3 targetMins )
		{
			var world = VoxelWorld.Current;

			SourceMins = sourceMins;
			SourceMaxs = sourceMaxs;
			TargetMins = targetMins;
			SourcePositions = world.GetPositionsInBox( SourceMins, SourceMaxs );

			var totalBlocks = SourcePositions.Count();
			var currentIndex = 0;

			TargetPositions = new IntVector3[totalBlocks];
			OldBlockStates = new BlockState[totalBlocks];
			OldBlockIds = new byte[totalBlocks];
			NewBlockStates = new BlockState[totalBlocks];
			NewBlockIds = new byte[totalBlocks];

			foreach ( var position in SourcePositions )
			{
				var localPosition = position - SourceMins;
				var newPosition = TargetMins + localPosition;

				TargetPositions[currentIndex] = newPosition;

				OldBlockStates[currentIndex] = world.GetState<BlockState>( newPosition );
				OldBlockIds[currentIndex] = world.GetBlock( newPosition );

				var state = world.GetState<BlockState>( position );

				if ( state.IsValid() )
				{
					NewBlockStates[currentIndex] = state.Copy();
				}

				NewBlockIds[currentIndex] = world.GetBlock( position );

				currentIndex++;
			}
		}

		public override void Perform()
		{
			var world = VoxelWorld.Current;

			for ( var i = 0; i < TargetPositions.Length; i++ )
			{
				world.SetBlockOnServer( TargetPositions[i], NewBlockIds[i] );
				world.SetState( TargetPositions[i], NewBlockStates[i] );
			}

			base.Perform();
		}

		public override void Undo()
		{
			var world = VoxelWorld.Current;

			for ( var i = 0; i < TargetPositions.Length; i++ )
			{
				world.SetBlockOnServer( TargetPositions[i], OldBlockIds[i] );
				world.SetState( TargetPositions[i], OldBlockStates[i] );
			}

			base.Undo();
		}
	}
}
