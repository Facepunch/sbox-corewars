
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars.UI
{
	public class TeamPlayerItem : Panel
	{
		public Player Player { get; private set; }
		public Label Name { get; private set; }
		public Label Kills { get; private set; }
		public Label Spacer { get; private set; }

		public void Update( Player player )
		{
			Player = player;

			DeleteChildren( true );

			Name = Add.Label( player.Client.Name, "name" );
			Kills = Add.Label( "0", "kills" );
			Spacer = Add.Label( "" );
		}

		public override void Tick()
		{
			if ( !Player.IsValid() )
			{
				if ( !IsDeleting )
				{
					Delete();
					return;
				}
			}

			Kills.Text = Player.Client.GetInt( "kills " ).ToString();

			base.Tick();
		}
	}

	[UseTemplate]
	public partial class TeamItem : Panel
	{
		private Panel Header { get; set; }
		private Panel Players { get; set; }
		private Panel Core { get; set; }
		private TeamCore Entity { get; set; }

		public void Update( TeamCore core )
		{
			Entity = core;
			Header.Style.BackgroundColor = core.Team.GetColor();

			var players = core.Team.GetPlayers();
			Players.DeleteChildren( true );

			foreach ( var player in players )
			{
				if ( player.Client.IsValid() )
				{
					var item = Players.AddChild<TeamPlayerItem>( "player" );
					item.Update( player );
				}
			}

			SetClass( "hidden", IsHidden() );
			SetClass( "destroyed", IsDestroyed() );

			BindClass( "hidden", IsHidden );
			BindClass( "destroyed", IsDestroyed );
		}

		private bool IsHidden()
		{
			return Entity.Team.GetPlayers().Count() == 0;
		}

		private bool IsDestroyed()
		{
			return Entity.LifeState == LifeState.Dead;
		}
	}
}
