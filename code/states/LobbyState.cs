using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public class LobbyState : BaseState
	{
		public override void OnEnter()
		{
			foreach ( var player in Entity.All.OfType<Player>() )
			{
				player.Respawn();
			}
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( Player player )
		{
			player.Respawn();
		}
	}
}
