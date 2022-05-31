using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Facepunch.CoreWars.Editor
{
	public class FlipBlocksAction : EditorAction
	{
		public override string Name => "Flip Blocks";
		 
		private BlockState[] NewBlockStates { get; set; }
		private byte[] NewBlockIds { get; set; }
		private BlockState[] OldBlockStates { get; set; }
		private byte[] OldBlockIds { get; set; }
		private IntVector3 Mins { get; set; }
		private IntVector3 Maxs { get; set; }
		private bool FromOrigin { get; set; }
		private bool FlipX { get; set; }
		private bool FlipY { get; set; }
		private int Width { get; set; }
		private int Height { get; set; }
		private int Depth { get; set; }

		public void Initialize( IntVector3 mins, IntVector3 maxs, bool flipX, bool flipY )
		{
			var world = VoxelWorld.Current;

			FlipX = flipX;
			FlipY = flipY;

			Mins = world.GetPositionMins( mins, maxs );
			Maxs = world.GetPositionMaxs( mins, maxs );

			Width = (Maxs.x - Mins.x) + 1;
			Height = (Maxs.y - Mins.y) + 1;
			Depth = (Maxs.z - Mins.z) + 1;

			var totalBlocks = Width * Height * Depth;

			OldBlockIds = new byte[totalBlocks];
			NewBlockIds = new byte[totalBlocks];
			OldBlockStates = new BlockState[totalBlocks];
			NewBlockStates = new BlockState[totalBlocks];

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var position = new IntVector3( x, y, z );
						var localPosition = GetLocalPosition( x, y, z );
						var oldIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );
						OldBlockIds[oldIndex] = world.GetBlock( position );
						OldBlockStates[oldIndex] = world.GetState<BlockState>( position );
					}
				}
			}

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var localPosition = GetLocalPosition( x, y, z );
						var newIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );
						var oldIndex = GetArrayIndex(
							FlipX ? (Width - localPosition.x - 1) : localPosition.x,
							FlipY ? (Height - localPosition.y - 1) : localPosition.y,
							localPosition.z
						);
						NewBlockStates[newIndex] = OldBlockStates[oldIndex];
						NewBlockIds[newIndex] = OldBlockIds[oldIndex];
					}
				}
			}
		}

		public override void Perform()
		{
			var world = VoxelWorld.Current;

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var position = new IntVector3( x, y, z );
						var localPosition = GetLocalPosition( x, y, z );
						var newIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );
						world.SetBlockOnServer( position, NewBlockIds[newIndex] );
						world.SetState( position, NewBlockStates[newIndex] );
					}
				}
			}

			base.Perform();
		}

		public override void Undo()
		{
			var world = VoxelWorld.Current;

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var position = new IntVector3( x, y, z );
						var localPosition = GetLocalPosition( x, y, z );
						var oldIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );
						world.SetBlockOnServer( position, OldBlockIds[oldIndex] );
						world.SetState( position, OldBlockStates[oldIndex] );
					}
				}
			}

			base.Undo();
		}

		private IntVector3 GetLocalPosition( int x, int y, int z )
		{
			return new IntVector3( Maxs.x - x, Maxs.y - y, Maxs.z - z );
		}

		private int GetArrayIndex( int x, int y, int z )
		{
			return x * Height * Depth + y * Depth + z;
		}
	}
}
