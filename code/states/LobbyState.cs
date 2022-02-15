using Facepunch.CoreWars.Voxel;
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
					Log.Info( player.LifeState );
					SpawnPlayerWhenReady( player );
				}
			}
		}

		public override void OnLeave()
		{

		}

		public override void OnPlayerJoined( Player player )
		{
			SpawnPlayerWhenReady( player );
		}

		private async void SpawnPlayerWhenReady( Player player )
		{
			while ( Map.Current.SuitableSpawnPositions.Count == 0 )
			{
				await GameTask.Delay( 50 );
			}

			if ( player.LifeState == LifeState.Alive ) return;

			player.Respawn();
		}
	}
}
