using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	[EditorTool( Title = "Mirror Blocks", Description = "Mirror an area of blocks" )]
	[Icon( "textures/ui/tools/mirrorblocks.png" )]
	public partial class MirrorBlocksTool : EditorTool
	{
		public enum MirrorStage
		{
			Select,
			Mirror
		}

		[Net] public MirrorStage Stage { get; set; } = MirrorStage.Select;

		private EditorAreaGhost AreaGhost { get; set; }
		private TimeUntil NextBlockPlace { get; set; }
		private Vector3? StartPosition { get; set; }
		private Vector3? EndPosition { get; set; }

		[ConCmd.Server( "mirror_blocks_cancel" )]
		public static void SendCancelCmd()
		{
			if ( ConsoleSystem.Caller.Pawn is EditorPlayer player )
			{
				if ( player.Tool is MirrorBlocksTool tool )
				{
					tool.Cancel();
				}
			}
		}

		[ConCmd.Server( "mirror_blocks_mirror" )]
		public static void SendMirrorCmd( bool flipX, bool flipY, bool fromOrigin )
		{
			if ( ConsoleSystem.Caller.Pawn is EditorPlayer player )
			{
				if ( player.Tool is MirrorBlocksTool tool )
				{
					tool.Mirror( flipX, flipY, fromOrigin );
				}
			}
		}

		public void Cancel()
		{
			if ( IsClient )
			{
				SendCancelCmd();
			}

			NextBlockPlace = 0.1f;
			StartPosition = null;
			EndPosition = null;
			Stage = MirrorStage.Select;
		}

		public void Mirror( bool flipX, bool flipY, bool fromOrigin )
		{
			if ( IsClient )
			{
				SendMirrorCmd( flipX, flipY, fromOrigin );
			}

			if ( IsServer && Stage == MirrorStage.Mirror )
			{
				var startVoxelPosition = VoxelWorld.Current.ToVoxelPosition( StartPosition.Value );
				var endVoxelPosition = VoxelWorld.Current.ToVoxelPosition( EndPosition.Value );

				if ( fromOrigin )
				{
					var action = new MirrorBlocksAction();
					action.Initialize( startVoxelPosition, endVoxelPosition, flipX, flipY );
					Player.Perform( action );
				}
				else
				{
					var action = new FlipBlocksAction();
					action.Initialize( startVoxelPosition, endVoxelPosition, flipX, flipY );
					Player.Perform( action );
				}
			}

			NextBlockPlace = 0.1f;
			StartPosition = null;
			EndPosition = null;
			Stage = MirrorStage.Select;
		}

		public override void Simulate( Client client )
		{
			var world = VoxelWorld.Current;

			if ( IsClient && world.IsValid() && AreaGhost.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
				var aimSourcePosition = world.ToSourcePosition( aimVoxelPosition );

				if ( Stage == MirrorStage.Select )
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
			Stage = MirrorStage.Select;
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

				if ( Stage == MirrorStage.Select )
				{
					if ( StartPosition.HasValue )
					{
						if ( IsClient )
						{
							EditorMirrorMenu.Open();
						}

						EndPosition = aimSourcePosition;
						Stage = MirrorStage.Mirror;
					}
					else
					{
						StartPosition = aimSourcePosition;
					}
				}

				NextBlockPlace = 0.1f;
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( NextBlockPlace )
			{
				Cancel();
			}
		}
	}
}
