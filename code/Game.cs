using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Voxel;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Game : Sandbox.Game
	{
		[Net] public StateSystem StateSystem { get; private set; }

		public static new Game Current { get; private set; }
		public static Hud Hud { get; private set; }

		public Game()
		{
			if ( IsServer )
			{
				StateSystem = new();
				StateSystem.Set( new LobbyState() );
			}

			if ( IsClient )
			{
				Hud = new Hud();
			}

			Current = this;
		}

		public void SetBlockInDirection( Vector3 origin, Vector3 direction, byte blockId )
		{
			var face = Map.Current.GetBlockInDirection( origin * (1.0f / Chunk.VoxelSize), direction.Normal, 10000, out var endPosition, out _ );
			if ( face == BlockFace.Invalid ) return;

			var position = blockId != 0 ? Map.GetAdjacentBlockPosition( endPosition, (int)face ) : endPosition;
			SetBlockOnServer( position.x, position.y, position.z, blockId );
		}

		public void SetBlockOnServer( int x, int y, int z, byte blockId )
		{
			Host.AssertServer();

			var position = new IntVector3( x, y, z );

			if ( Map.Current.SetBlockAndUpdate( position, blockId ) )
			{
				SetBlockOnClient( x, y, z, blockId );
			}
		}

		[ClientRpc]
		public void SetBlockOnClient( int x, int y, int z, byte blockId )
		{
			Host.AssertClient();

			Map.Current.SetBlockAndUpdate( new IntVector3( x, y, z ), blockId, true );
		}

		public virtual void PlayerRespawned( Player player )
		{
			StateSystem.Active?.OnPlayerRespawned( player );
		}

		public override void OnKilled( Entity pawn )
		{
			if ( pawn is not Player player ) return;

			StateSystem.Active?.OnPlayerKilled( player, player.LastDamageTaken );
		}

		public override void MoveToSpawnpoint( Entity pawn )
		{
			if ( pawn is not Player player ) return;

			var spawnpoints = All.OfType<PlayerSpawnpoint>()
				.Where( e => e.Team == player.Team )
				.ToList();

			if ( spawnpoints.Count > 0 )
			{
				var spawnpoint = Rand.FromList( spawnpoints );
				player.Transform = spawnpoint.Transform;
			}
		}

		public override bool CanHearPlayerVoice( Client sourceClient, Client destinationClient )
		{
			return false;
		}

		public override void DoPlayerNoclip( Client client ) { }

		public override void DoPlayerSuicide( Client client ) { }

		public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
		{
			StateSystem.Active?.OnPlayerDisconnected( client.Pawn as Player );
			base.ClientDisconnect( client, reason );
		}

		public override void ClientJoined( Client client )
		{
			var player = new Player( client );
			client.Pawn = player;

			StateSystem.Active?.OnPlayerJoined( player );

			Map.Current.Send( client );

			// For now just load every chunk in the map.
			foreach ( var chunk in Map.Current.Chunks )
			{
				player.LoadChunk( chunk );
			}

			base.ClientJoined( client );
		}

		public override void PostLevelLoaded()
		{
			if ( !IsServer )
				return;

			var map = new Map();

			map.SetSize( 256, 256, 64 );
			map.AddAllBlockTypes();
			map.GeneratePerlin();
			map.Init();

			Map.Current = map;
		}
	}
}
