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
				IResettable.ResetAll();

				foreach ( var player in Entity.All.OfType<Player>() )
				{
					player.AssignRandomTeam();
					player.RespawnWhenAvailable();
				}
			}
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( Player player )
		{
			player.AssignRandomTeam();
			player.RespawnWhenAvailable();
		}
	}
}
