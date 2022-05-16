using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
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
					var item = Players.Add.Panel( "player" );
					item.Add.Label( player.Client.Name, "name" );
					item.Add.Label( "0", "kills" );
					item.Add.Label( "" );
				}
			}
		}
	}
}
