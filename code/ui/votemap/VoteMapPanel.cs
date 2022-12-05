using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public class VoteMapPanel : Panel
	{
		public string TitleText { get; set; } = "Next Map Vote";
		public string SubtitleText { get; set; } = "Vote for the next map";
		public string TimeText { get; set; } = "00:00";

		public Panel Body { get; set; }
		public List<MapIcon> MapIcons = new();

		public VoteMapPanel()
		{
			_ = PopulateMaps();
		}

		public async Task PopulateMaps()
		{
			var result = await Package.FindAsync( "game:facepunch.corewars type:map order:user", 16 );
			var random = new Random();
			var randomPicks = result.Packages.OrderBy( x => random.Next() ).Take( 3 );

			foreach ( var package in randomPicks )
			{
				AddMap( package.FullIdent );
			}
		}

		private MapIcon AddMap( string fullIdent )
		{
			var icon = MapIcons.FirstOrDefault( x => x.Ident == fullIdent );

			if ( icon != null )
				return icon;

			icon = new MapIcon( fullIdent );
			icon.AddEventListener( "onmousedown", () => VoteMapEntity.SetVote( fullIdent ) );
			Body.AddChild( icon );

			MapIcons.Add( icon );
			return icon;
		}

		internal void UpdateFromVotes( IDictionary<Client, string> votes )
		{
			foreach ( var icon in MapIcons )
				icon.VoteCount = "0";

			foreach ( var group in votes.GroupBy( x => x.Value ).OrderByDescending( x => x.Count() ) )
			{
				var icon = AddMap( group.Key );
				icon.VoteCount = group.Count().ToString( "n0" );
			}
		}
	}
}
