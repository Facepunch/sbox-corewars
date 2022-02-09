﻿using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Voxel;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars
{
	public partial class Player : Sandbox.Player
	{
		[Net, Change( nameof( OnTeamChanged ) )] public Team Team { get; private set; }
		[BindComponent] public ChunkViewer ChunkViewer { get; }
		[Net] public byte CurrentBlockId { get; private set; }
		[Net] public List<byte> HotbarBlocks { get; private set; }
		[Net] public NetInventory MainInventory { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }

		public Player() : base()
		{

		}

		public Player( Client client ) : this()
		{
			CurrentBlockId = 1;
			HotbarBlocks = new List<byte>();

			for ( var i = 0; i < 8; i++ )
			{
				HotbarBlocks.Add( (byte)(i + 1) );
			}
		}

		[ServerCmd]
		public static void SetBlockId( int blockId )
		{
			if ( ConsoleSystem.Caller.Pawn is Player player )
			{
				player.CurrentBlockId = (byte)blockId;
			}
		}

		public async Task LoadChunkDelayed( Chunk chunk, int delayMs )
		{
			await GameTask.Delay( delayMs );
			LoadChunk( chunk );
		}

		public void LoadChunk( Chunk chunk )
		{
			if ( ChunkViewer.LoadedChunks.Contains( chunk.Index ) )
				return;

			ChunkViewer.LoadedChunks.Add( chunk.Index );

			var offset = chunk.Offset;
			var blocks = chunk.Blocks;
			var index = chunk.Index;

			ReceiveChunk( To.Single( Client ), offset.x, offset.y, offset.z, index, blocks, chunk.SerializeData() );
		}

		[ClientRpc]
		public void ReceiveChunk( int x, int y, int z, int index, byte[] blocks, byte[] data )
		{
			Map.Current.ReceiveChunk( index, blocks, data );

			var totalSize = (blocks.Length + data.Length) / 1024;
			Log.Info( $"(#{NetworkIdent}) Received all bytes for chunk{x},{y},{z} ({totalSize}kb)" );
		}

		public void SetTeam( Team team )
		{
			Host.AssertServer();

			Team = team;
			OnTeamChanged( team );
		}

		public void CreateInventory()
		{
			var container = new InventoryContainer( this );
			container.SetSlotLimit( 10 );
			container.AddConnection( Client );

			InventorySystem.Register( container );

			var grassBlocks = InventorySystem.CreateItem<BlockItem>();
			grassBlocks.MaxStackSize = 64;
			grassBlocks.StackSize = 64;
			grassBlocks.BlockId = Map.Current.FindBlockId<GrassBlock>();

			var stoneBlocks = InventorySystem.CreateItem<BlockItem>();
			stoneBlocks.MaxStackSize = 40;
			stoneBlocks.StackSize = 32;
			stoneBlocks.BlockId = Map.Current.FindBlockId<StoneBlock>();

			container.Give( grassBlocks, 2 );
			container.Give( stoneBlocks, 6 );

			var moreStoneBlocks = InventorySystem.CreateItem<BlockItem>();
			moreStoneBlocks.MaxStackSize = 40;
			moreStoneBlocks.StackSize = 16;
			moreStoneBlocks.BlockId = Map.Current.FindBlockId<StoneBlock>();

			container.Stack( moreStoneBlocks );

			MainInventory = new NetInventory( container );
		}

		public override void Spawn()
		{
			Components.Create<ChunkViewer>();

			EnableHideInFirstPerson = true;
			EnableAllCollisions = true;
			EnableDrawing = true;

			Camera = new FirstPersonCamera();

			Controller = new MoveController()
			{
				WalkSpeed = 195f,
				SprintSpeed = 375f
			};

			Animator = new PlayerAnimator();

			SetModel( "models/citizen/citizen.vmdl" );

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			foreach ( var item in MainInventory.Container.ItemList )
			{
				if ( item.IsValid() )
				{
					Log.Info( $"Received Initial Inventory Item {item.GetName()} @ Slot #{item.SlotId} (Stack: {item.StackSize})" );
				}
			}

			base.ClientSpawn();
		}

		public override void Respawn()
		{
			Game.Current?.PlayerRespawned( this );

			base.Respawn();
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );
		}

		public override void FrameSimulate( Client client )
		{
			base.FrameSimulate( client );
		}

		public override void Simulate( Client client )
		{
			if ( IsServer )
			{
				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					Map.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, CurrentBlockId );
				}
				else if ( Input.Pressed( InputButton.Attack2 ) )
				{
					Map.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, 0 );
				}
				else if ( Input.Pressed( InputButton.Flashlight ) )
				{
					Map.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var position );

					var data = Map.Current.GetOrCreateData<BlockData>( position );

					if ( data.Health == 0 )
						data.Health = 100;
					else
						data.Health--;

					data.IsDirty = true;
				}
			}
			else
			{
				if ( Input.Pressed( InputButton.Drop ) )
				{
					Map.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var position );

					var data = Map.Current.GetData<BlockData>( position );

					if ( data.IsValid() )
					{
						Log.Info( data.Health );
					}
				}
			}

			if ( IsClient && Prediction.FirstTime )
			{
				var currentSlotIndex = 0;

				for ( var i = 0; i < Hotbar.Current.Slots.Count; i++ )
				{
					if ( CurrentBlockId == Hotbar.Current.Slots[i].BlockId )
					{
						currentSlotIndex = i;
						break;
					}
				}

				if ( Input.MouseWheel > 0 )
					currentSlotIndex++;
				else if ( Input.MouseWheel < 0 )
					currentSlotIndex--;

				currentSlotIndex = Math.Clamp( currentSlotIndex, 0, Hotbar.Current.Slots.Count - 1 );

				var newBlockId = Hotbar.Current.Slots[currentSlotIndex].BlockId;

				if ( newBlockId != CurrentBlockId )
				{
					SetBlockId( newBlockId );
				}
			}

			if ( IsServer )
			{
				if ( Input.Released( InputButton.Reload ) )
				{
					ShuffleHotbarBlocks();
				}
				else if ( Input.Released( InputButton.Use) )
				{
					var random = Rand.Float();

					byte blockId;
					if ( random >= 0.66f )
						blockId = Map.Current.FindBlockId<RedTorchBlock>();
					else if ( random >= 0.33f )
						blockId = Map.Current.FindBlockId<GreenTorchBlock>();
					else
						blockId = Map.Current.FindBlockId<BlueTorchBlock>();

					Map.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, blockId );
				}
			}
			else if ( Map.Current.IsValid() )
			{
				var position = Map.Current.ToVoxelPosition( Input.Position );
				var voxel = Map.Current.GetVoxel( position );

				DebugOverlay.ScreenText( 2, $"Sunlight Level: {voxel.GetSunLight()}", 0.1f );
				DebugOverlay.ScreenText( 3, $"Torch Level: ({voxel.GetRedTorchLight( )}, {voxel.GetGreenTorchLight()}, {voxel.GetBlueTorchLight()})", 0.1f );
				DebugOverlay.ScreenText( 4, $"Chunk Index: {voxel.ChunkIndex}", 0.1f );
				DebugOverlay.ScreenText( 5, $"Position: {position}", 0.1f );
			}

			var controller = GetActiveController();
			controller?.Simulate( client, this, GetActiveAnimator() );
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );
		}

		public override void TakeDamage( DamageInfo info )
		{
			LastDamageTaken = info;

			base.TakeDamage( info );
		}

		protected virtual void OnTeamChanged( Team team )
		{

		}

		private void ShuffleHotbarBlocks()
		{
			var oldSlotIndex = 0;

			for ( var i = 0; i < HotbarBlocks.Count; i++ )
			{
				if ( CurrentBlockId == HotbarBlocks[i] )
				{
					oldSlotIndex = i;
					break;
				}
			}

			for ( var i = 0; i < HotbarBlocks.Count; i++ )
			{
				var randomBlock = Map.Current.BlockData.ElementAt( Rand.Int( 1, Map.Current.BlockData.Count - 1 ) ).Key;
				HotbarBlocks[i] = randomBlock;
			}

			CurrentBlockId = HotbarBlocks[oldSlotIndex];
		}
	}
}
