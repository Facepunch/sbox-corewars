using Sandbox;

namespace Facepunch.CoreWars
{
	public class BaseState : BaseNetworkable
	{
		public StateSystem System { get; set; }

		public virtual void OnEnter() { }

		public virtual void OnLeave() { }

		public virtual bool CanHearPlayerVoice( IClient a, IClient b )
		{
			return false;
		}

		public virtual void OnPlayerKilled( CoreWarsPlayer player, DamageInfo info ) { }

		public virtual void OnPlayerJoined( CoreWarsPlayer player ) { }

		public virtual void OnPlayerRespawned( CoreWarsPlayer player ) { }

		public virtual void OnPlayerDisconnected( CoreWarsPlayer player ) { }
	}
}
