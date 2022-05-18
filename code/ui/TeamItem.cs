using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
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
		public Panel Header { get; set; }
		public Panel Players { get; set; }
		public Panel Core { get; set; }
		public Team Team { get; set; }

		public void Update( Team team )
		{
			Team = team;
			Header.Style.BackgroundColor = team.GetColor();

			var players = team.GetPlayers();
			Players.DeleteChildren( true );

			foreach ( var player in players )
			{
				if ( player.Client.IsValid() )
				{
					var item = Players.AddChild<TeamPlayerItem>( "player" );
					item.Update( player );
				}
			}
		}
	}
}
