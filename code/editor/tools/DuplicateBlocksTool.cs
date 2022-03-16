using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	[EditorToolLibrary( Title = "Duplicate Blocks", Description = "Duplicate an area of blocks", Icon = "textures/ui/tools/duplicateblocks.png" )]
	public partial class DuplicateBlocksTool : EditorTool
	{
		public enum DuplicateStage
		{
			Copy,
			Paste
		}

		[Net] public DuplicateStage Stage { get; set; } = DuplicateStage.Copy;

		private EditorAreaGhost AreaGhost { get; set; }
		private TimeUntil NextBlockPlace { get; set; }
		private Vector3? StartPosition { get; set; }
		private Vector3? EndPosition { get; set; }

		public override void Simulate( Client client )
		{
			var currentMap = VoxelWorld.Current;

			if ( IsClient && currentMap.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				if ( Stage == DuplicateStage.Copy )
				{
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

					AreaGhost.Color = Color.Orange;
				}
				else
				{
					AreaGhost.MoveStartBlock( new BBox( aimSourcePosition, aimSourcePosition + new Vector3( VoxelWorld.Current.VoxelSize ) ) );
					AreaGhost.Color = Color.Green;
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
					Color = Color.Orange
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

		protected override void OnPrimary( Client client )
		{
			if ( NextBlockPlace )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				if ( Stage == DuplicateStage.Copy )
				{
					if ( StartPosition.HasValue )
					{
						EndPosition = aimSourcePosition;
						Stage = DuplicateStage.Paste;
					}
					else
					{
						StartPosition = aimSourcePosition;
					}
				}
				else
				{
					if ( IsServer )
					{
						var startSourceVoxelPosition = VoxelWorld.Current.ToVoxelPosition( StartPosition.Value );
						var endSourceVoxelPosition = VoxelWorld.Current.ToVoxelPosition( EndPosition.Value );

						foreach ( var position in VoxelWorld.Current.GetPositionsInBox( startSourceVoxelPosition, endSourceVoxelPosition ) )
						{
							var localPosition = position - startSourceVoxelPosition;
							var blockId = VoxelWorld.Current.GetBlock( position );
							var blockState = VoxelWorld.Current.GetState<BlockState>( position );
							var newPosition = aimVoxelPosition + localPosition;

							VoxelWorld.Current.SetBlockOnServer( newPosition, blockId );

							if ( blockState.IsValid() )
							{
								VoxelWorld.Current.SetState( newPosition, blockState.Copy() );
							}
						}
					}
				}

				NextBlockPlace = 0.1f;
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( NextBlockPlace )
			{
				if ( Stage == DuplicateStage.Paste )
				{
					StartPosition = null;
					Stage = DuplicateStage.Copy;
				}

				NextBlockPlace = 0.1f;
			}
		}
	}
}
