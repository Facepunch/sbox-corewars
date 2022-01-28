using Facepunch.CoreWars.Voxel;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Game : Sandbox.Game
	{
		[Net] public StateSystem StateSystem { get; private set; }
		[Net] public Map Map { get; private set; }

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

		public void SetBlockInDirection( Vector3 position, Vector3 direction, byte blockType )
		{
			var face = Map.GetBlockInDirection( position * (1.0f / Chunk.VoxelSize), direction.Normal, 10000, out var hitPosition, out _ );
			if ( face == Map.BlockFace.Invalid ) return;

			var blockPos = hitPosition;

			if ( blockType != 0 )
				blockPos = Map.GetAdjacentBlockPosition( blockPos, (int)face );

			SetBlockOnServer( blockPos.x, blockPos.y, blockPos.z, blockType );
		}

		public void SetBlockOnServer( int x, int y, int z, byte blockType )
		{
			Host.AssertServer();

			var pos = new IntVector3( x, y, z );

			if ( Map.SetBlockAndUpdate( pos, blockType ) )
			{
				Map.WriteNetworkDataForChunkAtPosition( pos );
				SetBlockOnClient( x, y, z, blockType );
			}
		}

		[ClientRpc]
		public void SetBlockOnClient( int x, int y, int z, byte blockType )
		{
			Host.AssertClient();

			Map.SetBlockAndUpdate( new IntVector3( x, y, z ), blockType, true );
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
			base.ClientJoined( client );
		}

		public override void PostLevelLoaded()
		{
			if ( !IsServer )
				return;

			Map = new Map();
			Map.SetSize( 256, 256, 64 );
			Map.GeneratePerlin();
			Map.Init();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Map.Init();
		}
	}
}
