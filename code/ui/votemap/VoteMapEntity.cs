using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class VoteMapEntity : Entity
	{
		private static VoteMapEntity Current { get; set; }

		private VoteMapPanel Panel { get; set; }

		[Net] public IDictionary<Client, string> Votes { get; set; }
		[Net] public string WinningMap { get; set; } = "facepunch.cw_newworld";
		[Net] public RealTimeUntil VoteTimeLeft { get; set; } = 30f;

		[ConCmd.Server( "cw_votemap" )]
		public static void SetVote( string map )
		{
			if ( Current == null || ConsoleSystem.Caller == null )
				return;

			Current.SetVote( ConsoleSystem.Caller, map );
		}

		public override void Spawn()
		{
			base.Spawn();

			Transmit = TransmitType.Always;
			Current = this;
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Current = this;
			Panel = new VoteMapPanel();

			Game.Hud.AddChild( Panel );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Panel?.Delete();
			Panel = null;

			if ( Current == this )
				Current = null;
		}

		[Event.Client.Frame]
		public void OnFrame()
		{
			if ( Panel != null )
			{
				var seconds = VoteTimeLeft.Relative.FloorToInt().Clamp( 0, 60 );
				Panel.TimeText = $"00:{seconds:00}";
			}
		}

		private void CullInvalidClients()
		{
			foreach ( var entry in Votes.Keys.Where( x => !x.IsValid() ).ToArray() )
			{
				Votes.Remove( entry );
			}
		}

		private void UpdateWinningMap()
		{
			if ( Votes.Count == 0 )
				return;

			WinningMap = Votes.GroupBy( x => x.Value ).OrderBy( x => x.Count() ).First().Key;
		}

		private void SetVote( Client client, string map )
		{
			CullInvalidClients();
			Votes[client] = map;

			UpdateWinningMap();
			RefreshUI();
		}

		[ClientRpc]
		private void RefreshUI()
		{
			Panel.UpdateFromVotes( Votes );
		}
	}
}
