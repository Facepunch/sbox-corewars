using Facepunch.Voxels;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace Facepunch.CoreWars.Editor
{
	public class MirrorBlocksAction : EditorAction
	{
		public override string Name => "Mirror Blocks";
		 
		private BlockState[] NewTargetBlockStates { get; set; }
		private byte[] NewTargetBlockIds { get; set; }
		private BlockState[] OldTargetBlockStates { get; set; }
		private byte[] OldTargetBlockIds { get; set; }
		private IntVector3 Mins { get; set; }
		private IntVector3 Maxs { get; set; }
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

			OldTargetBlockIds = new byte[totalBlocks];
			NewTargetBlockIds = new byte[totalBlocks];
			NewTargetBlockStates = new BlockState[totalBlocks];
			OldTargetBlockStates = new BlockState[totalBlocks];

			for ( var x = Mins.x; x <= Maxs.x; x++ )
			{
				for ( var y = Mins.y; y <= Maxs.y; y++ )
				{
					for ( var z = Mins.z; z <= Maxs.z; z++ )
					{
						var position = new IntVector3( x, y, z );
						var localPosition = GetLocalPosition( x, y, z );
						var oldIndex = GetArrayIndex( localPosition.x, localPosition.y, localPosition.z );

						NewTargetBlockIds[oldIndex] = world.GetBlock( position );

						var state = world.GetState<BlockState>( position );

						if ( state.IsValid() )
						{
							NewTargetBlockStates[oldIndex] = state.Copy();
						}

						var sourcePosition = world.ToSourcePositionCenter( position );
						var origin = world.ToSourcePositionCenter( world.MaxSize / 2 );
						var delta = world.ToSourcePositionCenter( position ) - origin;
						var mirrored = origin - delta;

						if ( FlipX ) sourcePosition.x = mirrored.x;
						if ( FlipY ) sourcePosition.y = mirrored.y;

						position = world.ToVoxelPosition( sourcePosition );

						OldTargetBlockIds[oldIndex] = world.GetBlock( position );
						OldTargetBlockStates[oldIndex] = world.GetState<BlockState>( position );
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
						var sourcePosition = world.ToSourcePositionCenter( position );
						var origin = world.ToSourcePositionCenter( world.MaxSize / 2 );
						var delta = world.ToSourcePositionCenter( position ) - origin;
						var mirrored = origin - delta;

						if ( FlipX ) sourcePosition.x = mirrored.x;
						if ( FlipY ) sourcePosition.y = mirrored.y;

						position = world.ToVoxelPosition( sourcePosition );

						world.SetBlockOnServer( position, NewTargetBlockIds[newIndex] );
						world.SetState( position, NewTargetBlockStates[newIndex] );
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
						var sourcePosition = world.ToSourcePositionCenter( position );
						var origin = world.ToSourcePositionCenter( world.MaxSize / 2 );
						var delta = world.ToSourcePositionCenter( position ) - origin;
						var mirrored = origin - delta;

						if ( FlipX ) sourcePosition.x = mirrored.x;
						if ( FlipY ) sourcePosition.y = mirrored.y;

						position = world.ToVoxelPosition( sourcePosition );

						world.SetBlockOnServer( position, OldTargetBlockIds[oldIndex] );
						world.SetState( position, OldTargetBlockStates[oldIndex] );
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
