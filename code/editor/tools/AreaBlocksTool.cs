using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System;

namespace Facepunch.CoreWars.Editor
{
	[EditorToolLibrary( Title = "Area Blocks", Description = "Create blocks in a defined area", Icon = "textures/ui/tools/areablocks.png" )]
	public class AreaBlocksTool : EditorTool
	{
		private EditorAreaGhost AreaGhost { get; set; }
		private TimeUntil NextBlockPlace { get; set; }
		private Vector3? StartPosition { get; set; }

		public override void Simulate( Client client )
		{
			var currentMap = VoxelWorld.Current;

			if ( IsClient && currentMap.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition();
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				if ( StartPosition.HasValue )
				{
					AreaGhost.StartBlock = new BBox( StartPosition.Value, StartPosition.Value + new Vector3( VoxelWorld.Current.VoxelSize ) );
					AreaGhost.EndBlock = new BBox( aimSourcePosition, aimSourcePosition + new Vector3( VoxelWorld.Current.VoxelSize ) );
					AreaGhost.UpdateRenderBounds();
				}
				else
				{
					AreaGhost.StartBlock = new BBox( aimSourcePosition, aimSourcePosition + new Vector3( VoxelWorld.Current.VoxelSize ) );
					AreaGhost.EndBlock = AreaGhost.StartBlock;
					AreaGhost.UpdateRenderBounds();
				}
			}

			base.Simulate( client );
		}

		public override void OnSelected()
		{
			if ( IsClient )
			{
				AreaGhost = new EditorAreaGhost
				{
					RenderBounds = new BBox( Vector3.One * -100f, Vector3.One * 100f ),
					EnableDrawing = true,
					Color = Color.Green
				};
			}
		}

		public override void OnDeselected()
		{
			if ( IsClient )
			{
				AreaGhost?.Delete();
			}
		}

		protected IntVector3 GetAimVoxelPosition()
		{
			var distance = VoxelWorld.Current.VoxelSize * 4f;
			var aimVoxelPosition = VoxelWorld.Current.ToVoxelPosition( Input.Position + Input.Rotation.Forward * distance );
			var face = VoxelWorld.Current.Trace( Input.Position * (1.0f / VoxelWorld.Current.VoxelSize), Input.Rotation.Forward, distance, out var endPosition, out _ );

			if ( face != BlockFace.Invalid && VoxelWorld.Current.GetBlock( endPosition ) != 0 )
			{
				var oppositePosition = VoxelWorld.GetAdjacentPosition( endPosition, (int)face );
				aimVoxelPosition = oppositePosition;
			}

			return aimVoxelPosition;
		}

		protected override void OnPrimary( Client client )
		{
			if ( NextBlockPlace )
			{
				var aimVoxelPosition = GetAimVoxelPosition();
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				if ( StartPosition.HasValue )
				{
					if ( IsServer )
					{
						var startVoxelPosition = VoxelWorld.Current.ToVoxelPosition( StartPosition.Value );
						var endVoxelPosition = aimVoxelPosition;

						foreach ( var position in VoxelWorld.Current.GetBlocksInBox( startVoxelPosition, endVoxelPosition ) )
						{
							VoxelWorld.Current.SetBlockOnServer( position, Player.SelectedBlockId );
						}
					}

					StartPosition = null;
				}
				else
				{
					StartPosition = aimSourcePosition;
				}

				NextBlockPlace = 0.1f;
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( NextBlockPlace )
			{
				var aimVoxelPosition = GetAimVoxelPosition();
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				if ( StartPosition.HasValue )
				{
					if ( IsServer )
					{
						var startVoxelPosition = VoxelWorld.Current.ToVoxelPosition( StartPosition.Value );
						var endVoxelPosition = aimVoxelPosition;

						foreach ( var position in VoxelWorld.Current.GetBlocksInBox( startVoxelPosition, endVoxelPosition ) )
						{
							VoxelWorld.Current.SetBlockOnServer( position, 0 );
						}
					}

					StartPosition = null;
				}
				else
				{
					StartPosition = aimSourcePosition;
				}

				NextBlockPlace = 0.1f;
			}
		}
	}
}
