using Facepunch.CoreWars.Blocks;
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

		public InventoryContainer MainInventory { get; private set; }
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

			ReceiveChunk( To.Single( Client ), offset.x, offset.y, offset.z, index, blocks, chunk.DataMap.Data );
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
			MainInventory = new InventoryContainer( this );
			MainInventory.SetSlotLimit( 10 );
			MainInventory.AddConnection( Client );

			InventorySystem.Register( MainInventory );

			MainInventory.Give( "test_item", 2 );
			MainInventory.Give( "test_item", 6 );

			SendInventoryToOwner();
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

			if ( IsServer && Prediction.FirstTime )
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
			else
			{
				Vector3 fPosition = Input.Position * (1.0f / Chunk.VoxelSize);
				IntVector3 intPosition = new IntVector3( (int)fPosition.x, (int)fPosition.y, (int)fPosition.z );
				DebugOverlay.ScreenText( 2, $"Sunlight Level: {Map.Current.GetSunLight(intPosition)}", 0.1f );
				DebugOverlay.ScreenText( 3, $"Torch Level: ({Map.Current.GetRedTorchLight( intPosition )}, {Map.Current.GetGreenTorchLight( intPosition )}, {Map.Current.GetBlueTorchLight( intPosition )})", 0.1f );
				DebugOverlay.ScreenText( 4, $"Chunk Index: {Map.Current.GetChunkIndex( intPosition )}", 0.1f );
				DebugOverlay.ScreenText( 5, $"Position: {intPosition}", 0.1f );
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

		private void SendInventoryToOwner()
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.WriteInventoryContainer( MainInventory );
					ReceiveInventory( To.Single( Client ), stream.GetBuffer() );
				}
			}
		}

		[ClientRpc]
		private void ReceiveInventory( byte[] data )
		{
			using ( var stream = new MemoryStream( data ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					MainInventory = reader.ReadInventoryContainer();

					foreach ( var item in MainInventory.ItemList )
					{
						if ( item.IsValid() )
						{
							Log.Info( $"Received Initial Inventory Item {item.UniqueName} @ Slot #{item.SlotId}" );
						}
					}
				}
			}
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
