using Sandbox;
using System.Linq;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public class GameState : BaseState
	{
		public Dictionary<long,Team> PlayerToTeam { get; set; } = new();

		public override void OnEnter()
		{
			if ( IsServer )
			{
				foreach ( var player in Entity.All.OfType<Player>() )
				{
					player.AssignRandomTeam( true );
					player.RespawnWhenAvailable();

					var playerId = player.Client.PlayerId;
					PlayerToTeam[playerId] = player.Team;
				}
			}
		}

		public override bool CanHearPlayerVoice( Client a, Client b )
		{
			if ( a.Pawn is not Player source )
				return false;

			if ( b.Pawn is not Player destination )
				return false;

			return source.Team == destination.Team;
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( Player player )
		{
			var playerId = player.Client.PlayerId;

			if ( !PlayerToTeam.ContainsKey( playerId ) )
				player.AssignRandomTeam( true );
			else
				player.SetTeam( PlayerToTeam[playerId] );

			PlayerToTeam[playerId] = player.Team;

			player.RespawnWhenAvailable();
		}
	}
}
