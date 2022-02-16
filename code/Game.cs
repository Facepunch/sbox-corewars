using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Game : Sandbox.Game
	{
		[Net] public StateSystem StateSystem { get; private set; }

		public static new Game Current { get; private set; }
		public static Hud Hud { get; private set; }

		[ServerVar( "cw_editor", Saved = true )]
		public static bool IsEditorMode { get; set; }

		public Game()
		{
			if ( IsServer )
			{
				StateSystem = new();
			}

			if ( IsClient )
			{
				Hud = new Hud();
			}

			Current = this;
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
			if ( Map.Current.IsValid() )
			{
				if ( IsEditorMode )
				{
					pawn.Position = (Map.Current.MaxSize * Map.Current.VoxelSize * 0.5f).WithZ( Map.Current.MaxSize.z * Map.Current.VoxelSize );
					return;
				}

				var spawnpoint = Rand.FromList( Map.Current.SuitableSpawnPositions );
				pawn.Position = spawnpoint;
				return;
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
			InventorySystem.ClientDisconnected( client );
			StateSystem.Active?.OnPlayerDisconnected( client.Pawn as Player );
			base.ClientDisconnect( client, reason );
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			if ( IsEditorMode )
			{
				var player = new EditorPlayer( client );
				player.LifeState = LifeState.Dead;
			}
			else
			{
				var player = new Player( client );
				player.LifeState = LifeState.Dead;
			}

			Map.Current.AddViewer( client );

			if ( Map.Current.Initialized )
			{
				SendMapToClient( client);
			}
		}

		public override void PostLevelLoaded()
		{
			if ( !IsServer )
				return;

			if ( IsEditorMode )
				StateSystem.Set( new EditorState() );
			else
				StateSystem.Set( new LobbyState() );

			StartLoadMapTask();
		}
		
		private async void StartLoadMapTask()
		{
			var map = Map.Create( 1337 );

			map.OnInitialized += OnMapInitialized;
			map.SetBuildCollisionInThread( true );
			map.SetVoxelMaterial( "materials/corewars/voxel.vmat" );
			map.SetChunkRenderDistance( 4 );
			map.SetChunkUnloadDistance( 8 );
			map.SetChunkSize( 32, 32, 32 );
			map.SetSeaLevel( 48 );
			map.SetMaxSize( 256, 256, 128 );
			map.LoadBlockAtlas( "textures/blocks.json" );
			map.AddAllBlockTypes();

			if ( !IsEditorMode )
			{
				map.SetMinimumLoadedChunks( 8 );
				map.SetChunkGenerator<PerlinChunkGenerator>();
				map.AddBiome<PlainsBiome>();
				map.AddBiome<WeirdBiome>();

				var startChunkSize = 4;

				for ( var x = 0; x < startChunkSize; x++ )
				{
					for ( var y = 0; y < startChunkSize; y++ )
					{
						await GameTask.Delay( 100 );

						var chunk = map.GetOrCreateChunk(
							x * map.ChunkSize.x,
							y * map.ChunkSize.y,
							0
						);

						_ = chunk.Initialize();
					}
				}
			}
			else
			{
				map.SetChunkRenderDistance( 4 );
				map.SetChunkUnloadDistance( 8 );
				map.SetChunkGenerator<EditorChunkGenerator>();
				map.AddBiome<EditorBiome>();
			}

			await GameTask.Delay( 500 );

			map.Init();
		}

		private void OnMapInitialized()
		{
			var clients = Client.All.ToList();

			foreach ( var client in clients )
			{
				SendMapToClient( client );
			}
		}

		private void SendMapToClient( Client client )
		{
			Map.Current.Send( client );

			if ( client.Pawn is Player )
			{
				var player = (client.Pawn as Player);
				StateSystem.Active?.OnPlayerJoined( player );
				player.OnMapLoaded();
			}
			else if ( client.Pawn is EditorPlayer )
			{
				var player = (client.Pawn as EditorPlayer);
				player.OnMapLoaded();
				player.Respawn();
			}
		}
	}
}
