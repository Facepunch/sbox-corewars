using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class LobbyState : BaseState
	{
		[Net] public RealTimeUntil StateEndTime { get; set; }
		public float StateDuration => 15f;

		public override void OnEnter()
		{
			if ( IsServer )
			{
				foreach ( var player in Entity.All.OfType<CoreWarsPlayer>() )
				{
					player.SetTeam( Team.None );
					player.RespawnWhenAvailable();
				}

				StateEndTime = StateDuration;
			}
		}

		public override void OnLeave()
		{

		}

		public override bool CanHearPlayerVoice( Client sourceClient, Client destinationClient )
		{
			return true;
		}

		public override void OnPlayerJoined( CoreWarsPlayer player )
		{
			player.RespawnWhenAvailable();
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( StateEndTime )
			{
				System.Set( new GameState() );
			}
		}
	}
}
