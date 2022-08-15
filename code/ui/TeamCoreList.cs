using Facepunch.CoreWars.Inventory;
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class TeamCoreList : Panel
	{
		public static TeamCoreList Current { get; private set; }

		private class TeamCoreIcon : Panel
		{
			private TeamCore Core { get; set; }

			public TeamCoreIcon()
			{
				AddClass( "core" );
			}

			public void SetCoreEntity( TeamCore core )
			{
				Style.BackgroundTint = core.Team.GetColor();
				Core = core;
			}

			public override void Tick()
			{
				SetClass( "destroyed", !Core.IsValid() || Core.LifeState == LifeState.Dead );
				base.Tick();
			}
		}

		private Panel Container { get; set; }

		protected override void PostTemplateApplied()
		{
			BindClass( "hidden", IsHidden );

			foreach ( var core in Entity.All.OfType<TeamCore>() )
			{
				var icon = new TeamCoreIcon();
				icon.SetCoreEntity( core );
				Container.AddChild( icon );
			}

			base.PostTemplateApplied();
		}

		private bool IsHidden()
		{
			if ( Local.Pawn.LifeState == LifeState.Dead )
				return true;

			if ( IDialog.IsActive() || !Game.IsState<GameState>() )
				return true;

			return false;
		}
	}
}
