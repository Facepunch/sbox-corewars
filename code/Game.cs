using Facepunch.CoreWars.Editor;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars
{
	public partial class Game : Sandbox.Game
	{
		[Net] public StateSystem StateSystem { get; private set; }
		[Net] public bool IsEditorMode { get; private set; }

		public static new Game Current { get; private set; }
		public static RootPanel Hud { get; private set; }

		private static HashSet<Team> ValidTeamSet = new();

		public static T GetStateAs<T>() where T : BaseState
		{
			return Current.StateSystem.Active as T;
		}

		public static bool IsState<T>() where T : BaseState
		{
			return Current.StateSystem.Active is T;
		}

		public static bool TryGetState<T>( out T state ) where T : BaseState
		{
			state = GetStateAs<T>();
			return (state != null);
		}

		public static void AddValidTeam( Team team )
		{
			ValidTeamSet.Add( team );
			TeamList.Refresh();
		}

		public static void RemoveValidTeam( Team team )
		{
			ValidTeamSet.Remove( team );
			TeamList.Refresh();
		}

		public static IReadOnlySet<Team> GetValidTeams()
		{
			return ValidTeamSet;
		}

		public Game()
		{
			if ( IsServer )
			{
				IsEditorMode = Global.MapName == "facepunch.cw_editor_map";
				StateSystem = new();
			}

			Current = this;
		}

		public override void ClientSpawn()
		{
			Hud = IsEditorMode ? new EditorHud() : new Hud();

			base.ClientSpawn();
		}

		[ConCmd.Server( "cw_editor_save" )]
		public static void SaveEditorMapCmd( string fileName )
		{
			if ( !fileName.StartsWith( "worlds/" ) )
				fileName = $"worlds/{fileName}";

			if ( !fileName.EndsWith( ".voxels" ) )
				fileName += ".voxels";

			FileSystem.Data.CreateDirectory( "worlds" );

			Log.Info( $"Saving voxel world to disk ({fileName})..." );
			VoxelWorld.Current.SaveToFile( FileSystem.Data, fileName );

			var state = GetStateAs<EditorState>();
			state.CurrentFileName = fileName;
		}

		[ConCmd.Server( "cw_editor_load" )]
		public static void LoadEditorMapCmd( string fileName )
		{
			_ = LoadEditorMapTask( fileName );
		}

		private static async Task LoadEditorMapTask( string fileName )
		{
			if ( !fileName.StartsWith( "worlds/" ) )
				fileName = $"worlds/{fileName}";

			if ( !fileName.EndsWith( ".voxels" ) )
				fileName += ".voxels";

			Log.Info( $"Loading voxel world from disk ({fileName})..." );

			FileSystem.Data.CreateDirectory( "worlds" );

			var success = await VoxelWorld.Current.LoadFromFile( FileSystem.Data, fileName );

			if ( !success )
			{
				Log.Error( $"Unable to load world from disk ({fileName}), file does not exist!" );
				return;
			}

			var state = GetStateAs<EditorState>();
			state.CurrentFileName = fileName;
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

		public override bool CanHearPlayerVoice( Client a, Client b )
		{
			return StateSystem.Active.CanHearPlayerVoice( a, b );
		}

		public override void DoPlayerNoclip( Client client ) { }

		public override void DoPlayerSuicide( Client client )
		{
			if ( client.Pawn is Player player )
			{
				player.TakeDamage( DamageInfo.Generic( 200f ) );
			}
		}

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
			world.SetMaterials( "materials/corewars/voxel.vmat", "materials/corewars/voxel_translucent.vmat" );
			world.SetChunkRenderDistance( 4 );
			world.SetChunkUnloadDistance( 8 );
			world.SetChunkSize( 32, 32, 32 );
			world.SetSeaLevel( 48 );
			world.SetMaxSize( 256, 256, 128 );
			world.LoadBlockAtlas( "textures/blocks/corewars/blocks.json" );
			world.AddAllBlockTypes();

			if ( !IsEditorMode )
			{
				world.SetMinimumLoadedChunks( 8 );

				var worldLoader = All.OfType<VoxelWorldLoader>().FirstOrDefault();

				if ( worldLoader.IsValid() )
				{
					world.SetChunkGenerator<EmptyChunkGenerator>();
					world.AddBiome<EmptyBiome>();

					var result = await world.LoadFromFile( FileSystem.Mounted, worldLoader.FileName );

					if ( !result )
					{
						throw new Exception( $"Unable to load the voxel world '{worldLoader.FileName}', file does not exist!" );
					}
				}
				else
				{
					throw new Exception( "Unable to locate a Voxel World Loader in this map!" );
				}
			}
			else
			{
				world.SetMinimumLoadedChunks( 4 );
				world.SetChunkRenderDistance( 4 );
				world.SetChunkUnloadDistance( 8 );
				world.SetChunkGenerator<EditorChunkGenerator>();
				world.AddBiome<EditorBiome>();
			}

			await GameTask.Delay( 500 );

			world.Initialize();
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
