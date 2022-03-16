using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	[EditorToolLibrary( Title = "Place Block", Description = "Place a block", Icon = "textures/ui/tools/placeblock.png" )]
	public class PlaceBlockTool : EditorTool
	{
		private EditorBlockGhost BlockGhost { get; set; }
		private TimeUntil NextBlockPlace { get; set; }

		public override void Simulate( Client client )
		{
			var currentMap = VoxelWorld.Current;

			if ( IsClient && currentMap.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );

				BlockGhost.Position = aimSourcePosition;
			}

			base.Simulate( client );
		}

		public override void OnSelected()
		{
			if ( IsClient )
			{
				BlockGhost = new EditorBlockGhost
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
				BlockGhost?.Delete();
			}
		}

		protected override void OnPrimary( Client client )
		{
			if ( IsServer && NextBlockPlace )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );
				VoxelWorld.Current.SetBlockOnServer( aimVoxelPosition, Player.SelectedBlockId );
				NextBlockPlace = 0.1f;
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( IsServer && NextBlockPlace )
			{
				if ( VoxelWorld.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var blockPosition ) )
				{
					var voxel = VoxelWorld.Current.GetVoxel( blockPosition );

					if ( voxel.IsValid )
					{
						VoxelWorld.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, 0 );
					}
				}

				NextBlockPlace = 0.1f;
			}
		}
	}
}
