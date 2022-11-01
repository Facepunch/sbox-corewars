using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;

namespace Facepunch.CoreWars.Editor
{
	[UseTemplate]
	public partial class EntityToolMenu : SimpleRadialMenu
	{
		public override InputButton Button => InputButton.Reload;

		public override void Populate()
		{
			AddAction( "Place", "Place an entity in the world", "textures/ui/tools/entities/place.png", () => SetToolMode( EntitiesToolMode.Place ) );
			AddAction( "Edit Data", "Edit the data of an entity", "textures/ui/tools/entities/edit.png", () => SetToolMode( EntitiesToolMode.DataEditor ) );
			AddAction( "Remove", "Remove an entity", "textures/ui/tools/entities/remove.png", () => SetToolMode( EntitiesToolMode.Remove ) );
			AddAction( "Move/Rotate", "Move or rotate an entity", "textures/ui/tools/entities/move.png", () => SetToolMode( EntitiesToolMode.MoveAndRotate ) );
			AddAction( "Entity List", "View available entities", "textures/ui/tools/entities/list.png", () => EditorEntityList.Open() );

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
