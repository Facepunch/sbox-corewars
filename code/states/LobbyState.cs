using Sandbox;
using System.Linq;

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
					player.Respawn();
					player.Position = new Vector3( 1000f, 1000f, 1000f );
				}
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
