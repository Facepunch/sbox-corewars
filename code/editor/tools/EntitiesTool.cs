using Facepunch.Voxels;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	[EditorToolLibrary( Title = "Entities", Description = "Add or manipulate entities", Icon = "textures/ui/tools/entities.png" )]
	public partial class EntitiesTool : EditorTool
	{
		public enum EntitiesToolMode
		{
			PlaceAndRemove,
			MoveAndRotate,
			DataEditor
		}

		private static EditorEntityLibraryAttribute CurrentEntityAttribute { get; set; }

		[Net] public EntitiesToolMode Mode { get; set; }

		public override void Simulate( Client client )
		{
			var currentMap = VoxelWorld.Current;

			if ( IsClient && currentMap.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );
				var aimSourcePosition = VoxelWorld.Current.ToSourcePosition( aimVoxelPosition );
			}

			base.Simulate( client );
		}

		public override void OnSelected()
		{
			if ( IsClient )
			{
				
			}
		}

		public override void OnDeselected()
		{
			if ( IsClient )
			{
				
			}
		}

		protected override void OnPrimary( Client client )
		{
			if ( IsServer )
			{
				
			}
		}

		protected override void OnSecondary( Client client )
		{
			if ( IsServer )
			{
				
			}
		}
	}
}
