using Sandbox;
using Sandbox.Internal;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace Facepunch.CoreWars
{
	[UseTemplate]
	public partial class WinSummary : Panel, IDialog
	{
		public static WinSummary Current { get; private set; }

		public Panel CoresDestroyedList { get; private set; }
		public Panel MostKillsList { get; private set; }
		public string NextRoundTime => Math.Max( TimeUntilNextRound.Relative, 0 ).ToString();
		public bool IsOpen { get; set; }

		private RealTimeUntil TimeUntilNextRound { get; set; }

		public WinSummary()
		{
			Current = this;
		}

		public void Open()
		{
			if ( IsOpen ) return;
			PlaySound( "itemstore.open" );
			IDialog.Activate( this );
			IsOpen = true;
		}

		public void Close()
		{
			if ( !IsOpen ) return;
			IDialog.Deactivate( this );
			IsOpen = false;
		}

		public void Populate( RealTimeUntil nextRoundTime )
		{
			TimeUntilNextRound = nextRoundTime;
			CoresDestroyedList.DeleteChildren( true );
			MostKillsList.DeleteChildren( true );

			var allValidClients = Client.All.Where( c => c.Pawn is Player player && player.Team != Team.None );
			var clientsByKills = allValidClients.ToList();
			clientsByKills.Sort( ( a, b ) => a.GetInt( "kills" ).CompareTo( b.GetInt( "kills" ) ) );

			var clientsByCoresDestroyed = allValidClients.ToList();
			clientsByCoresDestroyed.Sort( ( a, b ) => a.GetInt( "cores" ).CompareTo( b.GetInt( "cores" ) ) );

			for ( var i = 0; i < 4; i++ )
			{
				if ( clientsByKills.Count > i )
					AddRow( MostKillsList, clientsByKills[i], clientsByKills[i].GetInt( "kills" ) );
				else
					AddEmptyRow( MostKillsList );

				if ( clientsByCoresDestroyed.Count > i )
					AddRow( CoresDestroyedList, clientsByCoresDestroyed[i], clientsByCoresDestroyed[i].GetInt( "cores" ) );
				else
					AddEmptyRow( CoresDestroyedList );
			}
		}

		protected override void PostTemplateApplied()
		{
			base.PostTemplateApplied();

			BindClass( "hidden", () => !IsOpen );
		}

		private void AddEmptyRow( Panel parent )
		{
			var row = parent.AddChild<Panel>( "player" );
			row.AddChild<Panel>( "avatar" );
			row.Add.Label( string.Empty, "username" );
			row.Add.Label( string.Empty, "score" );
		}

		private void AddRow( Panel parent, Client client, int score )
		{
			var row = parent.AddChild<Panel>( "player" );
			var avatar = row.AddChild<Panel>( "avatar" );
			avatar.Style.SetBackgroundImage( $"avatar:{client.PlayerId}" );
			row.Add.Label( client.Name, "username" );
			row.Add.Label( score.ToString(), "score" );
		}
	}
}
