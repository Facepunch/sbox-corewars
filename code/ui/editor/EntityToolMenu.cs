using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[StyleSheet( "/ui/editor/EntityToolMenu.scss" )]
	public partial class EntityToolMenu : RadialMenu
	{
		public override InputButton Button => InputButton.Score;

		public override void Populate()
		{
			AddItem( "Place", "Place an entity in the world", "textures/ui/tools/entities/place.png", () => SetToolMode( EntitiesToolMode.Place ) );
			AddItem( "Edit Data", "Edit the data of an entity", "textures/ui/tools/entities/edit.png", () => SetToolMode( EntitiesToolMode.DataEditor ) );
			AddItem( "Remove", "Remove an entity", "textures/ui/tools/entities/remove.png", () => SetToolMode( EntitiesToolMode.Remove ) );
			AddItem( "Move/Rotate", "Move or rotate an entity", "textures/ui/tools/entities/move.png", () => SetToolMode( EntitiesToolMode.MoveAndRotate ) );
			AddItem( "Entity List", "View available entities", "textures/ui/tools/entities/list.png", () => EditorEntityList.Open() );

			base.Populate();
		}

		protected override bool ShouldOpen()
		{
			if ( Local.Pawn is not EditorPlayer player )
				return false;

			if ( Input.Down( InputButton.Duck ) )
				return false;

			return player.Tool is EntitiesTool;
		}

		private void SetToolMode( EntitiesToolMode mode )
		{
			if ( Local.Pawn is not EditorPlayer player )
				return;

			EntitiesTool.ChangeModeCmd( (int)mode );
		}
	}
}

