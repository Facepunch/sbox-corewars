﻿using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	public enum EntitiesToolMode
	{
		Place,
		Remove,
		MoveAndRotate,
		DataEditor
	}

	[EditorToolLibrary( Title = "Entities", Description = "Add or manipulate entities", Icon = "textures/ui/tools/entities.png" )]
	public partial class EntitiesTool : EditorTool
	{
		[ServerCmd]
		public static void ChangeLibraryAttributeCmd( string type )
		{
			if ( ConsoleSystem.Caller.Pawn is not EditorPlayer player )
				return;

			if ( player.Tool is not EntitiesTool tool )
				return;

			tool.SetLibraryAttribute( (EditorEntityLibraryAttribute)Library.GetAttribute( type ) );
		}

		[ServerCmd]
		public static void ChangeModeCmd( int mode )
		{
			if ( ConsoleSystem.Caller.Pawn is not EditorPlayer player )
				return;

			if ( player.Tool is not EntitiesTool tool )
				return;
			
			tool.SetMode( (EntitiesToolMode)mode );
		}

		private static EditorEntityLibraryAttribute CurrentEntityAttribute { get; set; }

		[Net, Change( nameof( OnModeChanged ))] public EntitiesToolMode Mode { get; private set; }
		[Net, Change( nameof( OnLibraryTypeChanged ))] public string CurrentLibraryType { get; private set; }

		private EditorEntityLibraryAttribute CurrentLibraryAttribute { get; set; }
		private ModelEntity GhostEntity { get; set; }

		public void SetLibraryAttribute( EditorEntityLibraryAttribute attribute )
		{
			Host.AssertServer();

			if ( CurrentLibraryType != attribute.Name )
			{
				CurrentLibraryType = attribute.Name;
				CurrentLibraryAttribute = attribute;
				OnLibraryAttributeChanged( CurrentLibraryAttribute );
			}
		}

		public void SetMode( EntitiesToolMode mode )
		{
			Host.AssertServer();

			if ( Mode != mode )
			{
				Mode = mode;
				OnModeChanged( mode );
			}
		}

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
				SetMode( EntitiesToolMode.Place );
			}

			if ( IsClient )
			{
				OnModeChanged( Mode );
			}
		}

		public override void OnDeselected()
		{
			if ( IsClient )
			{
				DestroyGhostEntity();
			}
		}

		protected virtual void OnLibraryTypeChanged( string type )
		{
			CurrentLibraryAttribute = (EditorEntityLibraryAttribute)Library.GetAttribute( type );
			OnLibraryAttributeChanged( CurrentLibraryAttribute );
		}

		protected virtual void OnLibraryAttributeChanged( EditorEntityLibraryAttribute attribute )
		{
			if ( IsClient )
			{
				if ( Mode == EntitiesToolMode.Place )
				{
					CreateGhostEntity();
				}
			}
		}

		protected virtual void OnModeChanged( EntitiesToolMode mode )
		{
			if ( IsClient )
			{
				if ( Mode == EntitiesToolMode.Place )
				{
					CurrentEntityAttribute = Library.GetAttributes<EditorEntityLibraryAttribute>().FirstOrDefault();
					CreateGhostEntity();
				}
				else
				{
					DestroyGhostEntity();
				}
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
