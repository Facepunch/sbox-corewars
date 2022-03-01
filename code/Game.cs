using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Game : Sandbox.Game
	{
		[Net] public StateSystem StateSystem { get; private set; }
		[Net] public bool IsEditorMode { get; private set; }

		public static new Game Current { get; private set; }
		public static RootPanel Hud { get; private set; }

		[ServerVar( "cw_editor", Saved = true )]
		public static bool EditorModeConVar { get; set; }

		public Game()
		{
			if ( IsServer )
			{
				IsEditorMode = EditorModeConVar;
				StateSystem = new();
			}

			Current = this;
		}

		public override void ClientSpawn()
		{
			Hud = IsEditorMode ? new EditorHud() : new Hud();

			base.ClientSpawn();
		}

		[ServerCmd( "cw_editor_save" )]
		public static void SaveEditorMapToDisk()
		{
			if ( Current.StateSystem.Active is EditorState state )
			{
				state.SaveChunksToDisk( VoxelWorld.Current );
			}
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
			if ( VoxelWorld.Current.IsValid() )
			{
				if ( IsEditorMode )
				{
					pawn.Position = (VoxelWorld.Current.MaxSize * VoxelWorld.Current.VoxelSize * 0.5f);
					return;
				}

				var spawnpoint = Rand.FromList( VoxelWorld.Current.SuitableSpawnPositions );
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

			VoxelWorld.Current.AddViewer( client );

			if ( VoxelWorld.Current.Initialized )
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
			var world = VoxelWorld.Create( 1337 );

			world.OnInitialized += OnMapInitialized;
			world.SetBuildCollisionInThread( true );
			world.SetVoxelMaterial( "materials/corewars/voxel.vmat" );
			world.SetChunkRenderDistance( 4 );
			world.SetChunkUnloadDistance( 8 );
			world.SetChunkSize( 32, 32, 32 );
			world.SetSeaLevel( 48 );
			world.SetMaxSize( 256, 256, 128 );
			world.LoadBlockAtlas( "textures/blocks.json" );
			world.AddAllBlockTypes();

			if ( !IsEditorMode )
			{
				world.SetMinimumLoadedChunks( 8 );
				world.SetChunkGenerator<PerlinChunkGenerator>();
				world.AddBiome<PlainsBiome>();
				world.AddBiome<WeirdBiome>();

				var startChunkSize = 4;

				for ( var x = 0; x < startChunkSize; x++ )
				{
					for ( var y = 0; y < startChunkSize; y++ )
					{
						await GameTask.Delay( 100 );

						var chunk = world.GetOrCreateChunk(
							x * world.ChunkSize.x,
							y * world.ChunkSize.y,
							0
						);

						_ = chunk.Initialize();
					}
				}
			}
			else
			{
				world.SetChunkRenderDistance( 4 );
				world.SetChunkUnloadDistance( 8 );
				world.SetChunkGenerator<EditorChunkGenerator>();
				world.AddBiome<EditorBiome>();

				var state = StateSystem.Active as EditorState;
				await state.LoadInitialChunks( world );
			}

			await GameTask.Delay( 500 );

			world.Init();
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
			VoxelWorld.Current.Send( client );

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
