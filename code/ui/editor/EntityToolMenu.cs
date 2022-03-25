﻿using Facepunch.CoreWars.Editor;
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
			AddAction( "Place", "Place an entity in the world", "textures/ui/blocklist.png", () => SetToolMode( EntitiesToolMode.Place ) );
			AddAction( "Edit Data", "Edit the data of an entity", "textures/ui/blocklist.png", () => SetToolMode( EntitiesToolMode.DataEditor ) );
			AddAction( "Remove", "Remove an entity", "textures/ui/blocklist.png", () => SetToolMode( EntitiesToolMode.Remove ) );
			AddAction( "Move/Rotate", "Move or rotate an entity", "textures/ui/blocklist.png", () => SetToolMode( EntitiesToolMode.MoveAndRotate ) );

			base.Populate();
		}

		protected override bool ShouldOpen()
		{
			if ( Local.Pawn is not EditorPlayer player )
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
