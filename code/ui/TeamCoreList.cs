
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars.UI
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
				var playerCount = Core.Team.GetPlayers().Count();
				SetClass( "destroyed", !Core.IsValid() || Core.LifeState == LifeState.Dead || playerCount == 0 );
				base.Tick();
			}
		}

		public TeamCoreList()
		{
			Current = this;
		}

		private Panel Container { get; set; }

		[Event.Entity.PostSpawn]
		public void Update()
		{
			Container.DeleteChildren( true );

			foreach ( var core in Entity.All.OfType<TeamCore>() )
			{
				var icon = new TeamCoreIcon();
				icon.SetCoreEntity( core );
				Container.AddChild( icon );
			}
		}

		protected override void PostTemplateApplied()
		{
			BindClass( "hidden", IsHidden );
			Update();

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
