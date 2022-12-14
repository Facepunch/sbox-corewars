using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	[EditorTool( Title = "Move Blocks", Description = "Move an area of blocks" )]
	[Icon( "textures/ui/tools/moveblocks.png" )]
	public partial class MoveBlocksTool : EditorTool
	{
		public enum MoveStage
		{
			Select,
			Move
		}

		[Net] public MoveStage Stage { get; set; } = MoveStage.Select;

		private EditorAreaGhost AreaGhost { get; set; }
		private TimeUntil NextBlockPlace { get; set; }
		private Vector3? StartPosition { get; set; }
		private Vector3? EndPosition { get; set; }

		public override void Simulate( IClient client )
		{
			var world = VoxelWorld.Current;

			if ( Game.IsClient && world.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
				var aimSourcePosition = world.ToSourcePosition( aimVoxelPosition );

				if ( Stage == MoveStage.Select )
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

			Event.Register( this );

			StartPosition = null;
			EndPosition = null;
			Stage = MoveStage.Select;
		}

		public override void OnDeselected()
		{
			base.OnDeselected();

			Event.Unregister( this );

			if ( Game.IsClient )
			{
				Player.EditorCamera.ZoomOut = 0f;
				VoxelWorld.Current.GlobalOpacity = 1f;
				AreaGhost?.Delete();
			}
		}

		[Event.Client.Frame]
		protected virtual void OnFrame()
		{
			var world = VoxelWorld.Current;

			if ( world.IsValid() && AreaGhost.IsValid() && Stage == MoveStage.Move )
			{
				var size = AreaGhost.WorldBBox.Size;
				var width = (size.x / world.VoxelSize).CeilToInt();
				var height = (size.y / world.VoxelSize).CeilToInt();
				var depth = (size.z / world.VoxelSize).CeilToInt();
				var center = AreaGhost.WorldBBox.Center;

				DebugOverlay.Text( $"Width: {width}", center + new Vector3( size.x * 0.5f, 0f, 0f ), Color.Red );
				DebugOverlay.Text( $"Height: {height}", center + new Vector3( 0f, size.y * 0.5f, 0f ), Color.Green );
				DebugOverlay.Text( $"Depth: {depth}", center + new Vector3( 0f, 0f, size.z * 0.5f ), Color.Cyan );
				DebugOverlay.Axis( center, Rotation.Identity, size.Length * 0.25f, 0f, false );
			}
		}

		protected override void OnPrimary( IClient client )
		{
			if ( NextBlockPlace )
			{
				var world = VoxelWorld.Current;
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
				var aimSourcePosition = world.ToSourcePosition( aimVoxelPosition );

				if ( Stage == MoveStage.Select )
				{
					if ( StartPosition.HasValue )
					{
						EndPosition = aimSourcePosition;
						Stage = MoveStage.Move;
					}
					else
					{
						StartPosition = aimSourcePosition;
					}
				}
				else
				{
					if ( Game.IsServer )
					{
						var startSourceVoxelPosition = world.ToVoxelPosition( StartPosition.Value );
						var endSourceVoxelPosition = world.ToVoxelPosition( EndPosition.Value );

						var action = new MoveBlocksAction();
						action.Initialize( startSourceVoxelPosition, endSourceVoxelPosition, aimVoxelPosition);

						Player.Perform( action );
					}

					NextBlockPlace = 0.1f;
					StartPosition = null;
					EndPosition = null;
					Stage = MoveStage.Select;
				}

				NextBlockPlace = 0.1f;
			}
		}

		protected override void OnSecondary( IClient client )
		{
			if ( NextBlockPlace )
			{
				NextBlockPlace = 0.1f;
				StartPosition = null;
				EndPosition = null;
				Stage = MoveStage.Select;
			}
		}
	}
}
