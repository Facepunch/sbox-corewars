using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Facepunch.CoreWars.Editor
{
	public class MoveBlocksAction : EditorAction
	{
		public override string Name => "Move Blocks";
		 
		private List<IntVector3> SourcePositions { get; set; }
		private BlockState[] OldTargetBlockStates { get; set; }
		private byte[] OldTargetBlockIds { get; set; }
		private BlockState[] OldSourceBlockStates { get; set; }
		private byte[] OldSourceBlockIds { get; set; }
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
			SourcePositions = world.GetPositionsInBox( SourceMins, SourceMaxs ).ToList();

			var totalBlocks = SourcePositions.Count;
			var currentIndex = 0;

			TargetPositions = new IntVector3[totalBlocks];
			OldSourceBlockStates = new BlockState[totalBlocks];
			OldSourceBlockIds = new byte[totalBlocks];
			OldTargetBlockStates = new BlockState[totalBlocks];
			OldTargetBlockIds = new byte[totalBlocks];

			foreach ( var position in SourcePositions )
			{
				var localPosition = position - SourceMins;
				var newPosition = TargetMins + localPosition;

				TargetPositions[currentIndex] = newPosition;

				OldSourceBlockStates[currentIndex] = world.GetState<BlockState>( position );
				OldSourceBlockIds[currentIndex] = world.GetBlock( position );

				OldTargetBlockStates[currentIndex] = world.GetState<BlockState>( newPosition );
				OldTargetBlockIds[currentIndex] = world.GetBlock( newPosition );

				currentIndex++;
			}
		}

		public override void Perform()
		{
			var world = VoxelWorld.Current;

			for ( var i = 0; i < TargetPositions.Length; i++ )
			{
				world.SetBlockOnServer( SourcePositions[i], 0 );
				world.SetState<BlockState>( SourcePositions[i], null );

				world.SetBlockOnServer( TargetPositions[i], OldSourceBlockIds[i] );
				world.SetState( TargetPositions[i], OldSourceBlockStates[i] );
			}

			base.Perform();
		}

		public override void Undo()
		{
			var world = VoxelWorld.Current;

			for ( var i = 0; i < TargetPositions.Length; i++ )
			{
				world.SetBlockOnServer( TargetPositions[i], OldTargetBlockIds[i] );
				world.SetState( TargetPositions[i], OldTargetBlockStates[i] );

				world.SetBlockOnServer( SourcePositions[i], OldSourceBlockIds[i] );
				world.SetState( SourcePositions[i], OldSourceBlockStates[i] );
			}

			base.Undo();
		}
	}
}
