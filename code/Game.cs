using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Voxel;
using Sandbox;
using System;
using System.Collections.Generic;
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

			if ( Map.Current.IsValid() )
			{
				var spawnpoint = Rand.FromList( Map.Current.SuitableSpawnPositions );
				player.Position = spawnpoint;
				return;
			}

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
			InventorySystem.ClientDisconnected( client );
			StateSystem.Active?.OnPlayerDisconnected( client.Pawn as Player );
			base.ClientDisconnect( client, reason );
		}

		public override async void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new Player( client );
			client.Pawn = player;
			player.CreateInventory();

			if ( Map.Current.Initialized )
			{
				SendMapToPlayer( player );
			}
		}

		public override async void PostLevelLoaded()
		{
			if ( !IsServer )
				return;

			var map = Map.Create();
			map.OnInitialized += OnMapInitialized;
			map.SetSize( 256, 256, 128 );
			map.LoadBlockAtlas( "textures/blocks.json" );
			map.AddAllBlockTypes();

			var config = new Map.PerlinGenerationConfig
			{
				GroundBlockId = map.FindBlockId<GrassBlock>(),
				UndergroundBlockIds = new byte[]
				{
					map.FindBlockId<StoneBlock>(),
					map.FindBlockId<GrassBlock>()
				},
				BedrockId = map.FindBlockId<StoneBlock>()
			};

			Log.Info( $"[Server] Creating perlin map with id #{map.FindBlockId<GrassBlock>()}" );
			await map.GeneratePerlin( config );
			Log.Info( $"[Server] Perlin map has been created successfully." );

			map.Init();
		}

		private void OnMapInitialized()
		{
			foreach ( var player in All.OfType<Player>() )
			{
				SendMapToPlayer( player );
			}
		}

		private void SendMapToPlayer( Player player )
		{
			Map.Current.Send( player.Client );

			StateSystem.Active?.OnPlayerJoined( player );

			player.LoadChunks( Map.Current.Chunks.ToList() );
			player.OnMapLoaded();
		}
	}
}
