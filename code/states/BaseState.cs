﻿using Sandbox;

namespace Facepunch.CoreWars
{
	public class BaseState : BaseNetworkable
	{
		public bool IsServer => Host.IsServer;
		public bool IsClient => Host.IsClient;

		public StateSystem System { get; set; }

		public virtual void OnEnter() { }

		public virtual void OnLeave() { }

		public virtual bool CanHearPlayerVoice( Client a, Client b )
		{
			return false;
		}

		public virtual void OnPlayerKilled( Player player, DamageInfo info ) { }

		public virtual void OnPlayerJoined( Player player ) { }

		public virtual void OnPlayerRespawned( Player player ) { }

		public virtual void OnPlayerDisconnected( Player player ) { }
	}
}
