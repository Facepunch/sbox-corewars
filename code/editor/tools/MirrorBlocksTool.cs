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
		public static void SendMirrorCmd( bool flipX, bool flipY, bool fromOrigin, int offsetX, int offsetY )
		{
			if ( ConsoleSystem.Caller.Pawn is EditorPlayer player )
			{
				if ( player.Tool is MirrorBlocksTool tool )
				{
					tool.Mirror( flipX, flipY, fromOrigin, offsetX, offsetY );
				}
			}
		}

		public void Cancel()
		{
			if ( Game.IsClient )
			{
				SendCancelCmd();
			}

			NextBlockPlace = 0.1f;
			StartPosition = null;
			EndPosition = null;
			Stage = MirrorStage.Select;
		}

		public void Mirror( bool flipX, bool flipY, bool fromOrigin, int offsetX, int offsetY )
		{
			if ( Game.IsClient )
			{
				SendMirrorCmd( flipX, flipY, fromOrigin, offsetX, offsetY );
			}

			if ( Game.IsServer && Stage == MirrorStage.Mirror )
			{
				var startVoxelPosition = VoxelWorld.Current.ToVoxelPosition( StartPosition.Value );
				var endVoxelPosition = VoxelWorld.Current.ToVoxelPosition( EndPosition.Value );

				if ( fromOrigin )
				{
					var action = new MirrorBlocksAction();
					action.Initialize( startVoxelPosition, endVoxelPosition, flipX, flipY, offsetX, offsetY );
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

		public override void Simulate( IClient client )
		{
			var world = VoxelWorld.Current;

			if ( Game.IsClient && world.IsValid() && AreaGhost.IsValid() )
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
			base.OnSelected();

			if ( Game.IsClient )
			{
				VoxelWorld.Current.GlobalOpacity = 0.8f;

				AreaGhost = new EditorAreaGhost
				{
					RenderBounds = new BBox( Vector3.One * -100f, Vector3.One * 100f ),
					EnableDrawing = true,
					Color = Color.Orange
				};

				Player.EditorCamera.ZoomOut = 1f;
			}

			StartPosition = null;
			EndPosition = null;
			Stage = MirrorStage.Select;
		}

		public override void OnDeselected()
		{
			base.OnDeselected();

			if ( Game.IsClient )
			{
				Player.EditorCamera.ZoomOut = 0f;
				VoxelWorld.Current.GlobalOpacity = 1f;
				AreaGhost?.Delete();
			}
		}

		protected override void OnPrimary( IClient client )
		{
			if ( NextBlockPlace )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				if ( Stage == MirrorStage.Select )
				{
					if ( StartPosition.HasValue )
					{
						if ( Game.IsClient )
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

		protected override void OnSecondary( IClient client )
		{
			if ( NextBlockPlace )
			{
				Cancel();
			}
		}
	}
}
