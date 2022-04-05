using Facepunch.Voxels;
using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars
{
	public class LobbyState : BaseState
	{
		public override void OnEnter()
		{
			if ( Host.IsServer )
			{
				foreach ( var player in Entity.All.OfType<Player>() )
				{
					player.RespawnWhenAvailable();
				}
			}
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( Player player )
		{
			player.RespawnWhenAvailable();
		}
	}
}
