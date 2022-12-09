using Facepunch.CoreWars.Blocks;
using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	[EditorTool( Title = "Place Block", Description = "Place a block" )]
	[Icon( "textures/ui/tools/placeblock.png" )]
	public class PlaceBlockTool : EditorTool
	{
		private EditorBlockGhost BlockGhost { get; set; }
		private TimeUntil NextBlockPlace { get; set; }

		public override void Simulate( Client client )
		{
			var world = VoxelWorld.Current;

			if ( IsClient && world.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );
				var aimSourcePosition = world.ToSourcePosition( aimVoxelPosition );

				BlockGhost.Position = aimSourcePosition;
			}

			base.Simulate( client );
		}

		public override void OnSelected()
		{
			base.OnSelected();

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
			base.OnDeselected();

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
				var direction = GetTargetBlockFace( 4f );

				var action = new PlaceBlockAction();
				action.Initialize( aimVoxelPosition, Player.SelectedBlockId, direction );

				Player.Perform( action );

				NextBlockPlace = 0.1f;
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( IsServer && NextBlockPlace )
			{
				var pawn = client.Pawn as CoreWarsPlayer;

				if ( VoxelWorld.Current.GetBlockInDirection( pawn.EyePosition, pawn.EyeRotation.Forward, out var blockPosition ) )
				{
					var voxel = VoxelWorld.Current.GetVoxel( blockPosition );

					if ( voxel.IsValid )
					{
						VoxelWorld.Current.SetBlockInDirection( pawn.EyePosition, pawn.EyeRotation.Forward, 0 );
					}
				}

				NextBlockPlace = 0.1f;
			}
		}
	}
}
