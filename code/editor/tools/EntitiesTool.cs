using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	[EditorToolLibrary( Title = "Entities", Description = "Add or manipulate entities", Icon = "textures/ui/tools/entities.png" )]
	public partial class EntitiesTool : EditorTool
	{
		public enum EntitiesToolMode
		{
			Place,
			Remove,
			MoveAndRotate,
			DataEditor
		}

		private static EditorEntityLibraryAttribute CurrentEntityAttribute { get; set; }

		[Net] public EntitiesToolMode Mode { get; set; }

		private ModelEntity GhostEntity { get; set; }

		public override void Simulate( Client client )
		{
			var currentMap = VoxelWorld.Current;

			if ( IsClient && currentMap.IsValid() )
			{
				var aimVoxelPosition = GetAimVoxelPosition( 4f );
				var aimSourcePosition = VoxelWorld.Current.ToSourcePositionCenter( aimVoxelPosition, true, true, false );

				if ( Mode == EntitiesToolMode.Place )
				{
					if ( GhostEntity.IsValid() )
					{
						GhostEntity.Position = aimSourcePosition;
					}
				}
			}

			base.Simulate( client );
		}

		public override void OnSelected()
		{
			if ( IsServer )
			{
				Mode = EntitiesToolMode.Place;
			}

			if ( IsClient )
			{
				if ( Mode == EntitiesToolMode.Place )
				{
					CurrentEntityAttribute = Library.GetAttributes<EditorEntityLibraryAttribute>().FirstOrDefault();
					CreateGhostEntity();
				}
			}
		}

		public override void OnDeselected()
		{
			if ( IsClient )
			{
				DestroyGhostEntity();
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

		private void DestroyGhostEntity()
		{
			GhostEntity?.Delete();
			GhostEntity = null;
		}

		private void CreateGhostEntity()
		{
			DestroyGhostEntity();

			if ( CurrentEntityAttribute == null )
				return;

			GhostEntity = new ModelEntity( CurrentEntityAttribute.EditorModel );
			GhostEntity.RenderColor = Color.White.WithAlpha( 0.5f );
		}
	}
}
