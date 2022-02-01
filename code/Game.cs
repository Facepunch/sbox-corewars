using Facepunch.CoreWars.Blocks;
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

		public void SetBlockInDirection( Vector3 origin, Vector3 direction, byte blockType )
		{
			var face = Map.GetBlockInDirection( origin * (1.0f / Chunk.VoxelSize), direction.Normal, 10000, out var endPosition, out _ );
			if ( face == BlockFace.Invalid ) return;

			var position = blockType != 0 ? Map.GetAdjacentBlockPosition( endPosition, (int)face ) : endPosition;
			SetBlockOnServer( position.x, position.y, position.z, blockType );
		}

		public void SetBlockOnServer( int x, int y, int z, byte blockType )
		{
			Host.AssertServer();

			var position = new IntVector3( x, y, z );

			if ( Map.SetBlockAndUpdate( position, blockType ) )
			{
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

			// For now just load every chunk in the map.
			foreach ( var chunk in Map.Chunks )
			{
				player.LoadChunk( chunk );
			}

			base.ClientJoined( client );
		}

		public override void PostLevelLoaded()
		{
			if ( !IsServer )
				return;

			Map = new Map();
			Map.SetSize( 256, 256, 64 );
			Map.AddBlockType( new AirBlock() );
			Map.AddBlockType( new DirtBlock() );
			Map.AddBlockType( new SandBlock() );
			Map.AddBlockType( new StoneBlock() );
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
