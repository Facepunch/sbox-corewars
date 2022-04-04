using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;
using System;

namespace Facepunch.CoreWars.Editor
{
	[EditorTool( Title = "Area Blocks", Description = "Create blocks in a defined area", Icon = "textures/ui/tools/areablocks.png" )]
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
				var aimVoxelPosition = GetAimVoxelPosition( 6f );
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
				VoxelWorld.Current.GlobalOpacity = 0.8f;

				AreaGhost = new EditorAreaGhost
				{
					RenderBounds = new BBox( Vector3.One * -100f, Vector3.One * 100f ),
					EnableDrawing = true,
					Color = Color.Green
				};

				Player.Camera.ZoomOut = 1f;
			}
		}

		public override void OnDeselected()
		{
			if ( IsClient )
			{
				VoxelWorld.Current.GlobalOpacity = 1f;
				Player.Camera.ZoomOut = 0f;
				AreaGhost?.Delete();
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
