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
		public ProjectileSimulator Projectiles { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }
		public TimeUntil NextBlockPlace { get; private set; }

		public Player() : base()
		{
			Projectiles = new( this );
		}

		public Player( Client client ) : this()
		{
			CurrentHotbarIndex = 0;
			client.Pawn = this;
			CreateInventories();
		}

		public bool TryGiveWeapon( string weaponName )
		{
			var item = InventorySystem.CreateItem<WeaponItem>();
			var weapon = Library.Create<Weapon>( weaponName );
			weapon.OnCarryStart( this );
			item.Weapon = weapon;
			return HotbarInventory.Container.Give( item );
		}

		public void TryGiveAmmo( AmmoType type, ushort amount )
		{
			var item = InventorySystem.CreateItem<AmmoItem>();
			item.AmmoType = type;
			item.StackSize = amount;
			item.MaxStackSize = 60;
			HotbarInventory.Container.Stack( item );
		}

		public void TryGiveBlock( byte blockId, ushort amount )
		{
			var item = InventorySystem.CreateItem<BlockItem>();
			item.BlockId = blockId;
			item.StackSize = amount;
			item.MaxStackSize = 1000;
			HotbarInventory.Container.Stack( item );
		}

		public ushort TakeAmmo( AmmoType type, ushort count )
		{
			var items = HotbarInventory.Container.FindItems<AmmoItem>();
			var amountLeftToTake = count;
			ushort totalAmountTaken = 0;

			for ( int i = items.Count - 1; i >= 0; i-- )
			{
				var item = items[i];

				if ( item.AmmoType == type )
				{
					if ( item.StackSize >= amountLeftToTake )
					{
						totalAmountTaken += amountLeftToTake;
						item.StackSize -= amountLeftToTake;

						if ( item.StackSize > 0 )
							return totalAmountTaken;
					}
					else
					{
						amountLeftToTake -= item.StackSize;
						totalAmountTaken += item.StackSize;
						item.StackSize = 0;
					}

					HotbarInventory.Container.Remove( item.ItemId );
				}
			}

			return totalAmountTaken;
		}

		public int GetAmmoCount( AmmoType type )
		{
			var items = HotbarInventory.Container.FindItems<AmmoItem>();
			var output = 0;

			foreach ( var item in items )
			{
				if ( item.AmmoType == type )
				{
					output += item.StackSize;
				}
			}

			return output;
		}

		public void SetTeam( Team team )
		{
			Host.AssertServer();

			Team = team;
			OnTeamChanged( team );
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

			GiveInitialItems();
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

					NextBlockPlace = 0.1f;
				}
				else if ( Input.Down( InputButton.Attack2 ) && NextBlockPlace )
				{
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

					NextBlockPlace = 0.1f;
				}

				if ( Input.Pressed( InputButton.Flashlight ) )
				{
					Map.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var position );

					var radius = 8;

					for ( var x = -radius; x < radius; x++ )
					{
						for ( var y = -radius; y < radius; y++ )
						{
							for ( var z = -radius; z < radius; z++ )
							{
								var blockPosition = position + new IntVector3( x, y, z );

								if ( position.Distance( blockPosition ) <= radius )
								{
									Map.Current.SetBlockOnServer( blockPosition, 0, 0 );
								}
							}
						}
					}
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

				if ( IsServer )
				{
					var item = HotbarInventory.Container.GetFromSlot( CurrentHotbarIndex );

					if ( item is WeaponItem weaponItem )
						ActiveChild = weaponItem.Weapon;
					else
						ActiveChild = null;
				}
			}

			var currentMap = Map.Current;

			if ( IsServer )
			{
				if ( Input.Released( InputButton.Use) )
				{
					var random = Rand.Float();

					byte blockId;
					if ( random >= 0.66f )
						blockId = currentMap.FindBlockId<RedTorchBlock>();
					else if ( random >= 0.33f )
						blockId = currentMap.FindBlockId<GreenTorchBlock>();
					else
						blockId = currentMap.FindBlockId<BlueTorchBlock>();

					currentMap.SetBlockInDirection( Input.Position, Input.Rotation.Forward, blockId );
				}
			}
			else if ( currentMap.IsValid() )
			{
				var position = currentMap.ToVoxelPosition( Input.Position );
				var voxel = currentMap.GetVoxel( position );

				if ( voxel.IsValid )
				{
					DebugOverlay.ScreenText( 2, $"Sunlight Level: {voxel.GetSunLight()}", 0.1f );
					DebugOverlay.ScreenText( 3, $"Torch Level: ({voxel.GetRedTorchLight()}, {voxel.GetGreenTorchLight()}, {voxel.GetBlueTorchLight()})", 0.1f );
					DebugOverlay.ScreenText( 4, $"Chunk: {voxel.Chunk.Offset}", 0.1f );
					DebugOverlay.ScreenText( 5, $"Position: {position}", 0.1f );
					DebugOverlay.ScreenText( 6, $"Biome: {Map.Current.GetBiomeAt( position.x, position.y ).Name}", 0.1f );
				}
			}

			Projectiles.Simulate();

			SimulateActiveChild( client, ActiveChild );

			if ( !currentMap.IsValid() ) return;

			var voxelPosition = currentMap.ToVoxelPosition( Position );
			var currentChunk = currentMap.GetChunk( voxelPosition );

			if ( currentChunk.IsValid() && !currentChunk.HasDoneFirstFullUpdate )
				return;

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

		protected virtual void GiveInitialItems()
		{
			TryGiveBlock( Map.Current.FindBlockId<GrassBlock>(), 1000 );
			TryGiveBlock( Map.Current.FindBlockId<StoneBlock>(), 1000 );
			TryGiveBlock( Map.Current.FindBlockId<StoneBlock>(), 1000 );

			TryGiveWeapon( "weapon_boomer" );
			TryGiveAmmo( AmmoType.Explosive, 200 );
		}

		public virtual void CreateInventories()
		{
			var container = new InventoryContainer( this );
			container.SetSlotLimit( 8 );
			container.AddConnection( Client );
			InventorySystem.Register( container );

			HotbarInventory = new NetInventory( container );
		}
	}
}
