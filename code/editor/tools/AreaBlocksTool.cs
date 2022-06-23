using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System;
using static Facepunch.CoreWars.Editor.MoveBlocksTool;

namespace Facepunch.CoreWars.Editor
{
	[EditorTool( Title = "Area Blocks", Description = "Create blocks in a defined area" )]
	[Icon( "textures/ui/tools/areablocks.png" )]
	public class AreaBlocksTool : EditorTool
	{
		private EditorAreaGhost AreaGhost { get; set; }
		private TimeUntil NextBlockPlace { get; set; }
		private Vector3? StartPosition { get; set; }

		public override void Simulate( Client client )
		{
			var world = VoxelWorld.Current;

			if ( IsClient && world.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
				var aimSourcePosition = world.ToSourcePosition( aimVoxelPosition );

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
			base.OnSelected();

			if ( IsClient )
			{
				VoxelWorld.Current.GlobalOpacity = 0.8f;

				AreaGhost = new EditorAreaGhost
				{
					RenderBounds = new BBox( Vector3.One * -100f, Vector3.One * 100f ),
					EnableDrawing = true,
					Color = Color.Green
				};

				Player.Camera.ZoomOut = 1f;
			}

			Event.Register( this );
		}

		public override void OnDeselected()
		{
			base.OnDeselected();

			if ( IsClient )
			{
				VoxelWorld.Current.GlobalOpacity = 1f;
				Player.Camera.ZoomOut = 0f;
				AreaGhost?.Delete();
			}

			Event.Unregister( this );
		}

		[Event.Frame]
		protected virtual void OnFrame()
		{
			var world = VoxelWorld.Current;

			if ( world.IsValid() && AreaGhost.IsValid() )
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

		protected override void OnPrimary( Client client )
		{
			if ( NextBlockPlace )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				if ( StartPosition.HasValue )
				{
					if ( IsServer )
					{
						var startVoxelPosition = VoxelWorld.Current.ToVoxelPosition( StartPosition.Value );
						var endVoxelPosition = aimVoxelPosition;

						var action = new AreaBlocksAction();
						action.Initialize( startVoxelPosition, endVoxelPosition, Player.SelectedBlockId );

						Player.Perform( action );
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
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				if ( StartPosition.HasValue )
				{
					if ( IsServer )
					{
						var startVoxelPosition = VoxelWorld.Current.ToVoxelPosition( StartPosition.Value );
						var endVoxelPosition = aimVoxelPosition;

						var action = new AreaBlocksAction();
						action.Initialize( startVoxelPosition, endVoxelPosition, 0 );

						Player.Perform( action );
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
