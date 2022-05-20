using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	[EditorTool( Title = "Duplicate Blocks", Description = "Duplicate an area of blocks" )]
	[Icon( "textures/ui/tools/duplicateblocks.png" )]
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
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
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
				VoxelWorld.Current.GlobalOpacity = 0.8f;

				AreaGhost = new EditorAreaGhost
				{
					RenderBounds = new BBox( Vector3.One * -100f, Vector3.One * 100f ),
					EnableDrawing = true,
					Color = Color.Orange
				};

				Player.Camera.ZoomOut = 1f;
			}

			StartPosition = null;
			EndPosition = null;
			Stage = DuplicateStage.Copy;
		}

		public override void OnDeselected()
		{
			if ( IsClient )
			{
				Player.Camera.ZoomOut = 0f;
				VoxelWorld.Current.GlobalOpacity = 1f;
				AreaGhost?.Delete();
			}
		}

		protected override void OnPrimary( Client client )
		{
			if ( NextBlockPlace )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
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

						var action = new DuplicateBlocksAction();
						action.Initialize( startSourceVoxelPosition, endSourceVoxelPosition, aimVoxelPosition, Input.Down( InputButton.Run ) );

						Player.Perform( action );
					}
				}

				NextBlockPlace = 0.1f;
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( NextBlockPlace )
			{
				NextBlockPlace = 0.1f;
				StartPosition = null;
				Stage = DuplicateStage.Copy;
			}
		}
	}
}
