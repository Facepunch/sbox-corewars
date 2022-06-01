﻿using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	[EditorTool( Title = "Block Data", Description = "Edit the data of a block" )]
	[Icon( "textures/ui/tools/blockdata.png" )]
	public class BlockDataTool : EditorTool
	{
		private TimeUntil NextActionTime { get; set; }

		public override void OnSelected()
		{
			base.OnSelected();

			Event.Register( this );

			NextActionTime = 0.1f;
		}

		public override void OnDeselected()
		{
			base.OnDeselected();

			Event.Unregister( this );
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			var position = GetTargetBlockPosition();
			if ( !position.HasValue ) return;

			var world = VoxelWorld.Current;
			var blockId = world.GetBlock( position.Value );
			var block = world.GetBlockType( blockId );
			var outlineColor = Color.Cyan;
			var sourceBBox = world.ToSourceBBox( position.Value );

			DebugOverlay.Box(
				sourceBBox.Mins,
				sourceBBox.Maxs,
				outlineColor,
				Time.Delta,
				false
			);

			DebugOverlay.Text( $"Edit {block.FriendlyName}", sourceBBox.Center, Color.Cyan, Time.Delta );
		}

		protected override void OnPrimary( Client client )
		{
			if ( NextActionTime )
			{
				var position = GetTargetBlockPosition();

				if ( IsClient && position.HasValue )
				{
					EditorBlockData.SendOpenRequest( position.Value.x, position.Value.y, position.Value.z );
				}

				NextActionTime = 0.1f;
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( IsServer )
			{

			}
		}

		private IntVector3? GetTargetBlockPosition()
		{
			var face = VoxelWorld.Current.Trace( Input.Position * (1.0f / VoxelWorld.Current.VoxelSize), Input.Rotation.Forward, 1000f, out var endPosition, out _ );

			if ( face != BlockFace.Invalid && VoxelWorld.Current.GetBlock( endPosition ) != 0 )
			{
				return endPosition;
			}

			return null;
		}
	}
}
