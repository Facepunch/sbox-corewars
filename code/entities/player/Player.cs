using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Inventory;
using Facepunch.CoreWars.Voxel;
using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Facepunch.CoreWars
{
	public partial class Player : Sandbox.Player
	{
		[Net, Change( nameof( OnTeamChanged ) )] public Team Team { get; private set; }
		[BindComponent] public ChunkViewer ChunkViewer { get; }
		[Net, Predicted] public ushort CurrentHotbarIndex { get; private set; }
		[Net] public NetInventory HotbarInventory { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }
		public TimeUntil NextBlockPlace { get; private set; }

		public Player() : base()
		{

		}

		public Player( Client client ) : this()
		{
			CurrentHotbarIndex = 0;
		}

		public async Task LoadChunkDelayed( Chunk chunk, int delayMs )
		{
			await GameTask.Delay( delayMs );
			LoadChunk( chunk );
		}

		public void LoadChunks( List<Chunk> chunks )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.Write( chunks.Count );

					foreach ( var chunk in chunks )
					{
						writer.Write( chunk.Index );
						writer.Write( chunk.Blocks );

						chunk.SerializeData( writer );
					}

					var compressed = CompressionHelper.Compress( stream.ToArray() );
					Map.ReceiveChunks( To.Single( Client ), compressed );
				}
			}
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

		public void TryGiveBlock( byte blockId, ushort amount )
		{
			var item = InventorySystem.CreateItem<BlockItem>();
			item.BlockId = blockId;
			item.StackSize = amount;
			item.MaxStackSize = 1000;
			HotbarInventory.Container.Stack( item );
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
			container.SetSlotLimit( 8 );
			container.AddConnection( Client );

			InventorySystem.Register( container );

			HotbarInventory = new NetInventory( container );

			TryGiveBlock( Map.Current.FindBlockId<GrassBlock>(), 1000 );
			TryGiveBlock( Map.Current.FindBlockId<StoneBlock>(), 1000 );
			TryGiveBlock( Map.Current.FindBlockId<StoneBlock>(), 1000 );
		}

		public virtual void OnMapLoaded()
		{
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
		}

		public override void Spawn()
		{
			Components.Create<ChunkViewer>();
			EnableDrawing = false;
			base.Spawn();
		}

		public override void ClientSpawn()
		{
			if ( IsLocalPawn )
			{
				Hotbar.Current?.SetContainer( HotbarInventory.Container );
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
			if ( !Map.Current.IsValid() ) return;

			if ( IsServer )
			{
				if ( Input.Down( InputButton.Attack1 ) && NextBlockPlace )
				{
					NextBlockPlace = 0.1f;

					var container = HotbarInventory.Container;
					var item = container.GetFromSlot( CurrentHotbarIndex );

					if ( item.IsValid() && item is BlockItem blockItem )
					{
						var success = Map.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, blockItem.BlockId, true );

						if ( success )
						{
							item.StackSize--;

							if ( item.StackSize <= 0 )
							{
								InventorySystem.RemoveItem( item );
							}
						}
					}
				}
				else if ( Input.Down( InputButton.Attack2 ) && NextBlockPlace )
				{
					NextBlockPlace = 0.1f;

					if ( Map.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var blockPosition ) )
					{
						var voxel = Map.Current.GetVoxel( blockPosition );

						if ( voxel.IsValid )
						{
							if ( Map.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, 0 ) )
							{
								TryGiveBlock( voxel.BlockId, 1 );
							}
						}
					}
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
				else if ( Input.Pressed( InputButton.Score ) )
				{
					if ( Controller is MoveController )
					{
						EnableAllCollisions = false;
						Controller = new FlyingController();
					}
					else
					{
						EnableAllCollisions = true;
						Controller = new MoveController
						{
							WalkSpeed = 195f,
							SprintSpeed = 375f
						};
					}
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

			if ( Prediction.FirstTime )
			{
				var currentSlotIndex = (int)CurrentHotbarIndex;

				if ( Input.MouseWheel > 0 )
					currentSlotIndex++;
				else if ( Input.MouseWheel < 0 )
					currentSlotIndex--;

				var maxSlotIndex = HotbarInventory.Container.SlotLimit - 1;

				if ( currentSlotIndex < 0 )
					currentSlotIndex = maxSlotIndex;
				else if ( currentSlotIndex > maxSlotIndex )
					currentSlotIndex = 0;

				CurrentHotbarIndex = (ushort)currentSlotIndex;
			}

			if ( IsServer )
			{
				if ( Input.Released( InputButton.Use) )
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
				var position = Map.ToVoxelPosition( Input.Position );
				var voxel = Map.Current.GetVoxel( position );

				DebugOverlay.ScreenText( 2, $"Sunlight Level: {voxel.GetSunLight()}", 0.1f );
				DebugOverlay.ScreenText( 3, $"Torch Level: ({voxel.GetRedTorchLight( )}, {voxel.GetGreenTorchLight()}, {voxel.GetBlueTorchLight()})", 0.1f );
				DebugOverlay.ScreenText( 4, $"Chunk Index: {voxel.ChunkIndex}", 0.1f );
				DebugOverlay.ScreenText( 5, $"Position: {position}", 0.1f );

				//System.Threading.ThreadPool.GetAvailableThreads( out var available, out var cpThreads );

				//DebugOverlay.ScreenText( 6, $"Threads Available: {available}", 0.1f );
				//DebugOverlay.ScreenText( 7, $"Completion Pool Threads: {cpThreads}", 0.1f );
			}

			var voxelPosition = Map.ToVoxelPosition( Position );

			if ( Map.Current.IsValid() && Map.Current.IsInside( voxelPosition ) )
			{
				var currentChunkIndex = Map.Current.GetChunkIndex( voxelPosition );
				var currentChunk = Map.Current.Chunks[currentChunkIndex];

				if ( currentChunk.IsValid() && !currentChunk.HasDoneFirstFullUpdate )
				{
					return;
				}
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
	}
}
