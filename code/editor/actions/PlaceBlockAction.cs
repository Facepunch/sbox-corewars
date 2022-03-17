﻿using Facepunch.Voxels;

namespace Facepunch.CoreWars.Editor
{
	public class PlaceBlockAction : EditorAction
	{
		public override string Name => "Place Block";

		private IntVector3 Position { get; set; }
		private BlockState OldBlockState { get; set; }
		private byte OldBlockId { get; set; }
		private byte BlockId { get; set; }

		public void Initialize( IntVector3 position, byte blockId )
		{
			var world = VoxelWorld.Current;
			OldBlockState = world.GetState<BlockState>( position );
			OldBlockId = world.GetBlock( position );
			Position = position;
			BlockId = blockId;
		}

		public override void Perform()
		{
			var world = VoxelWorld.Current;
			world.SetBlockOnServer( Position, BlockId );

			base.Perform();
		}

		public override void Undo()
		{
			var world = VoxelWorld.Current;
			world.SetBlockOnServer( Position, OldBlockId );
			world.SetState( Position, OldBlockState );

			base.Undo();
		}
	}
}
