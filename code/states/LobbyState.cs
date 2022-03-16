using Facepunch.Voxels;
using Sandbox;
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
			while ( VoxelWorld.Current.SuitableSpawnPositions.Count == 0 )
			{
				try
				{
					await GameTask.Delay( 50 );
				}
				catch ( TaskCanceledException )
				{
					break;
				}
			}

			if ( player.LifeState == LifeState.Alive ) return;

			player.Respawn();
		}
	}
}
