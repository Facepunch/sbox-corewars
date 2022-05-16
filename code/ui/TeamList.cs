﻿using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class TeamList : Panel
	{
		public static TeamList Current { get; private set; }
		public Panel Container { get; set; }
		public bool IsOpen { get; private set; }

		public TeamList()
		{
			Current = this;
		}

		public static void Open()
		{
			Current?.SetOpen( true );
		}

		public static void Close()
		{
			Current?.SetOpen( false );
		}

		[ClientRpc]
		public static void Refresh()
		{
			if ( Current == null ) return;

			Current.Container.DeleteChildren( true );

			var teams = Entity.All.OfType<Player>()
				.Where( p => p.LifeState == LifeState.Alive )
				.Where( p => p.Team != Team.None )
				.Select( p => p.Team )
				.Distinct();

			foreach ( var team in teams )
			{
				var item = Current.Container.AddChild<TeamItem>();
				item.Update( team );
			}
		}

		public void SetOpen( bool isOpen )
		{
			IsOpen = isOpen;
		}

		public override void Tick()
		{
			base.Tick();
		}

		protected override void PostTemplateApplied()
		{
			if ( Local.Pawn is not Player player )
				return;

			BindClass( "hidden", IsHidden );

			Refresh();

			base.PostTemplateApplied();
		}

		private bool IsHidden()
		{
			if ( Local.Pawn.LifeState == LifeState.Dead )
				return true;

			if ( IDialog.IsActive() || !Game.IsState<GameState>() )
				return true;

			return !IsOpen;
		}
	}
}