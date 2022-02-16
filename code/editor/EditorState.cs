using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars.Editor
{
	public class EditorState : BaseState
	{
		public override void OnEnter()
		{
			if ( Host.IsServer )
			{
				foreach ( var player in Entity.All.OfType<Player>() )
				{
					player.Respawn();
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
