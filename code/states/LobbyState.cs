using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class LobbyState : BaseState
	{
		[Net] public RealTimeUntil StateEndTime { get; set; }
		public float StateDuration => 60f;

		public override void OnEnter()
		{
			if ( IsServer )
			{
				IResettable.ResetAll();

				foreach ( var player in Entity.All.OfType<Player>() )
				{
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

		public override void OnPlayerJoined( Player player )
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
