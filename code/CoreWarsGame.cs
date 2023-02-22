using Facepunch.CoreWars.Editor;
using Facepunch.Voxels;
using Sandbox;
using Sandbox.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars
{
	public partial class CoreWarsGame : GameManager
	{
		public static CoreWarsGame Entity => Current as CoreWarsGame;

		public static bool IsEditorMode => Entity?.InernalIsEditorMode ?? false;
		public static StateSystem StateSystem => Entity?.InernalStateSystem;

		[Net] private StateSystem InernalStateSystem { get; set; }
		[Net] private bool InernalIsEditorMode { get; set; }

		[ConVar.Server( "cw_friendly_fire", Saved = true )]
		public static bool FriendlyFire { get; set; } = false;

		private ScreenEffects PostProcessing { get; set; }

		private static readonly HashSet<Team> ValidTeamSet = new();

		public static T GetStateAs<T>() where T : BaseState
		{
			return StateSystem.Active as T;
		}

		public static bool IsState<T>() where T : BaseState
		{
			return StateSystem.Active is T;
		}

		public static bool TryGetState<T>( out T state ) where T : BaseState
		{
			state = GetStateAs<T>();
			return (state != null);
		}

		public static void AddValidTeam( Team team )
		{
			ValidTeamSet.Add( team );
		}

		public static void RemoveValidTeam( Team team )
		{
			ValidTeamSet.Remove( team );
		}

		public static IReadOnlySet<Team> GetValidTeams()
		{
			return ValidTeamSet;
		}

		public CoreWarsGame() : base()
		{
			if ( Game.IsClient )
			{
				PostProcessing = new();

				Camera.Main.RemoveAllHooks();
				Camera.Main.AddHook( PostProcessing );
			}
		}

		public override void Spawn()
		{
			InernalIsEditorMode = Game.Server.MapIdent == "facepunch.cw_editor_map";
			InernalStateSystem = new();

			InventorySystem.Initialize();
			base.Spawn();
		}

		public override void ClientSpawn()
		{
			InventorySystem.Initialize();

			ItemTag.Register( "remove_on_death", "Soulbound", Color.Green );
			ItemTag.Register( "uses_stamina", "Uses Stamina", Color.Cyan );
			ItemTag.Register( "droppable", "Droppable", Color.Yellow );

			Game.RootPanel?.Delete( true );
			Game.RootPanel = InernalIsEditorMode ? new EditorHud() : new UI.Hud();

			base.ClientSpawn();
		}

		[ConCmd.Server( "cw_end_game" )]
		public static void EndGameCmd()
		{
			Entity.InernalStateSystem.Set( new SummaryState() );
		}

		[ConCmd.Server( "cw_core_revive" )]
		public static void ReviveCoreCmd()
		{
			if ( ConsoleSystem.Caller.Pawn is CoreWarsPlayer player )
			{
				if ( player.Core.IsValid() )
				{
					player.Core.Reset();
				}
			}
		}

		[ConCmd.Server( "cw_explode_core" )]
		public static void ExplodeCoreCmd()
		{
			if ( ConsoleSystem.Caller.Pawn is CoreWarsPlayer player )
			{
				if ( player.Core.IsValid() )
				{
					player.Core.Explode();
				}
			}
		}

		[ConCmd.Server( "cw_editor_save" )]
		public static void SaveEditorMapCmd( string fileName )
		{
			SaveEditorMap( fileName );
		}

		[ConCmd.Server( "cw_change_team")]
		public static void ChangeTeamCmd( string teamIndex )
		{
			var team = Enum.Parse<Team>( teamIndex );

			if ( ConsoleSystem.Caller.Pawn is CoreWarsPlayer player )
			{
				player.SetTeam( team );
				Log.Info( "Changed team to: " + team.ToString() );
			}
		}

		[ConCmd.Server( "cw_announcement" )]
		public static void AddAnnouncementCmd( string title, string text )
		{
			UI.Announcements.Send( To.Everyone, title, text, RoundStage.Start.GetIcon() );
		}

		[ConCmd.Client( "cw_add_kill_feed" )]
		public static void AddKillFeedCmd( bool suicide )
		{
			if ( suicide )
			{
				UI.ToastList.Instance.AddKillFeed( Game.LocalPawn as CoreWarsPlayer, true );
			}
			else
			{
				UI.ToastList.Instance.AddKillFeed( Game.LocalPawn as CoreWarsPlayer, Game.LocalPawn as CoreWarsPlayer, (Game.LocalPawn as CoreWarsPlayer).ActiveChild );
			}
		}

		[ConCmd.Server( "cw_editor_load" )]
		public static void LoadEditorMapCmd( string fileName )
		{
			_ = LoadEditorMapTask( fileName );
		}

		public static void SaveEditorMap( string fileName, bool autosave = false )
		{
			if ( autosave && !fileName.Contains( "_autosave" ) )
			{
				if ( fileName.EndsWith( ".voxels" ) )
				{
					fileName = fileName.Replace( ".voxels", "" );
				}

				fileName = $"{fileName}_autosave";
			}

			if ( !fileName.StartsWith( "worlds/" ) )
				fileName = $"worlds/{fileName}";

			if ( !fileName.EndsWith( ".voxels" ) )
				fileName += ".voxels";

			FileSystem.Data.CreateDirectory( "worlds" );

			Log.Info( $"Saving voxel world to disk ({fileName})..." );
			VoxelWorld.Current.SaveToFile( FileSystem.Data, fileName );

			if ( !autosave )
			{
				var state = GetStateAs<EditorState>();
				state.CurrentFileName = fileName;
				EditorHud.ToastAll( "Saving...", "textures/ui/autosave.png" );
			}
			else
			{
				EditorHud.ToastAll( "Autosaving (Backup)...", "textures/ui/autosave.png" );
			}
		}

		private static async Task LoadEditorMapTask( string fileName )
		{
			if ( !fileName.StartsWith( "worlds/" ) )
				fileName = $"worlds/{fileName}";

			if ( !fileName.EndsWith( ".voxels" ) )
				fileName += ".voxels";

			Log.Info( $"Loading voxel world from disk ({fileName})..." );

			FileSystem.Data.CreateDirectory( "worlds" );

			var test = await VoxelWorld.Current.LoadFromFile( FileSystem.Data, fileName );

			if ( !test )
			{
				Log.Error( $"Unable to load world from disk ({fileName}), file does not exist!" );
				return;
			}

			var state = GetStateAs<EditorState>();
			state.CurrentFileName = fileName;
		}

		public virtual void PlayerRespawned( CoreWarsPlayer player )
		{
			StateSystem.Active?.OnPlayerRespawned( player );
		}

		public override void OnKilled( Entity pawn )
		{
			if ( pawn is not CoreWarsPlayer player ) return;

			StateSystem.Active?.OnPlayerKilled( player, player.LastDamageTaken );
		}

		public override bool CanHearPlayerVoice( IClient a, IClient b )
		{
			return StateSystem.Active.CanHearPlayerVoice( a, b );
		}

		public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
		{
			InventorySystem.ClientDisconnected( client );
			StateSystem.Active?.OnPlayerDisconnected( client.Pawn as CoreWarsPlayer );
			base.ClientDisconnect( client, reason );
		}

		public override void ClientJoined( IClient client )
		{
			base.ClientJoined( client );

			if ( IsEditorMode )
			{
				var player = new EditorPlayer( client );
				player.LifeState = LifeState.Dead;
			}
			else
			{
				var player = new CoreWarsPlayer( client );
				player.LifeState = LifeState.Dead;
			}

			VoxelWorld.Current.AddViewer( client );

			if ( VoxelWorld.Current.Initialized )
			{
				SendMapToClient( client);
			}
		}

		public override void RenderHud()
		{
			var pawn = Game.LocalPawn as CoreWarsPlayer;
			if ( !pawn.IsValid() ) return;

			pawn.RenderHud( Screen.Size );

			foreach ( var entity in All.OfType<IHudRenderer>() )
			{
				if ( entity.IsValid() )
				{
					entity.RenderHud( Screen.Size );
				}
			}
		}

		public override void PostLevelLoaded()
		{
			if ( !Game.IsServer )
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
			world.LoadBlockAtlas( "textures/blocks/corewars/blocks_color.atlas.json" );
			world.AddAllBlockTypes();

			if ( !IsEditorMode )
			{
				world.SetMinimumLoadedChunks( 8 );

				var worldLoader = All.OfType<VoxelWorldLoader>().FirstOrDefault();

				if ( worldLoader.IsValid() )
				{
					world.SetChunkGenerator<EmptyChunkGenerator>();
					world.AddBiome<EmptyBiome>();

					var fileName = worldLoader.FileName;
					var fs = FileSystem.Mounted;

					if ( !fs.FileExists( fileName ) && !fileName.EndsWith( ".json" ) )
					{
						fileName = $"{fileName}.json";
					}

					var result = await world.LoadFromFile( fs, fileName );

					if ( !result )
					{
						throw new Exception( $"Unable to load the voxel world '{fileName}', file does not exist!" );
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

		[Event.Client.Frame]
		private void OnFrame()
		{
			if ( Game.LocalPawn is not CoreWarsPlayer player )
				return;

			var pp = PostProcessing;

			pp.ChromaticAberration.Scale = 0.1f;
			pp.ChromaticAberration.Offset = Vector3.Zero;

			pp.Sharpen = 0.1f;

			var healthScale = (0.4f / player.MaxHealth) * player.Health;
			pp.Saturation = 0.7f + healthScale;

			pp.Vignette.Intensity = 0.8f - healthScale * 2f;
			pp.Vignette.Color = Color.Red.WithAlpha( 0.1f );
			pp.Vignette.Smoothness = 1f;
			pp.Vignette.Roundness = 0.8f;

			var sum = ScreenShake.List.OfType<ScreenShake.Random>().Sum( s => (1f - s.Progress) );

			PostProcessing.Pixelation = 0.02f * sum;
			PostProcessing.ChromaticAberration.Scale += (0.05f * sum);
		}

		private void OnMapInitialized()
		{
			var clients = Game.Clients.ToList();

			foreach ( var client in clients )
			{
				SendMapToClient( client );
			}
		}

		private void SendMapToClient( IClient client )
		{
			VoxelWorld.Current.Send( client );

			if ( client.Pawn is CoreWarsPlayer )
			{
				var player = (client.Pawn as CoreWarsPlayer);
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
