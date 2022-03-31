using Facepunch.CoreWars.Blocks;
using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
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
		[Net, Predicted] public ushort CurrentHotbarIndex { get; private set; }
		[Net] public NetInventory BackpackInventory { get; private set; }
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
			item.WeaponName = weaponName;
			
			if ( HotbarInventory.Container.Give( item ) )
				return true;

			return BackpackInventory.Container.Give( item );
		}

		public void TryGiveAmmo( AmmoType type, ushort amount )
		{
			var item = InventorySystem.CreateItem<AmmoItem>();
			item.AmmoType = type;
			item.StackSize = amount;
			item.MaxStackSize = 60;

			var remaining = HotbarInventory.Container.Stack( item );

			if ( remaining > 0 )
			{
				BackpackInventory.Container.Stack( item );
			}
		}

		public void TryGiveBlock( byte blockId, ushort amount )
		{
			var item = InventorySystem.CreateItem<BlockItem>();
			item.BlockId = blockId;
			item.StackSize = amount;
			item.MaxStackSize = 1000;

			var remaining = HotbarInventory.Container.Stack( item );

			if ( remaining > 0 )
			{
				BackpackInventory.Container.Stack( item );
			}
		}

		public ushort TakeAmmo( AmmoType type, ushort count )
		{
			var items = new List<AmmoItem>();

			items.AddRange( HotbarInventory.Container.FindItems<AmmoItem>() );
			items.AddRange( BackpackInventory.Container.FindItems<AmmoItem>() );

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

					item.Container.Remove( item.ItemId );
				}
			}

			return totalAmountTaken;
		}

		public int GetAmmoCount( AmmoType type )
		{
			var items = new List<AmmoItem>();

			items.AddRange( HotbarInventory.Container.FindItems<AmmoItem>() );
			items.AddRange( BackpackInventory.Container.FindItems<AmmoItem>() );

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

		public virtual Transform? GetSpawnpoint()
		{
			var world = VoxelWorld.Current;
			if ( !world.IsValid() ) return null;

			var spawnpoints = All.OfType<PlayerSpawnpoint>().ToList();

			if ( spawnpoints.Count == 0 )
			{
				if ( world.Spawnpoints.Count == 0 )
					return null;

				var spawnpoint = Rand.FromList( world.Spawnpoints );
				return new Transform( spawnpoint );
			}

			var randomSpawnpoint = Rand.FromList( spawnpoints );
			return randomSpawnpoint.Transform;
		}

		public virtual void OnMapLoaded()
		{
			EnableHideInFirstPerson = true;
			EnableAllCollisions = true;
			EnableDrawing = true;

			CameraMode = new FirstPersonCamera();

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
			EnableDrawing = false;
			base.Spawn();
		}

		public override void ClientSpawn()
		{
			if ( IsLocalPawn )
			{
				Hotbar.Current?.SetContainer( HotbarInventory.Container );
				Backpack.Current?.SetContainer( BackpackInventory.Container );
			}

			base.ClientSpawn();
		}

		public override void Respawn()
		{
			var spawnpoint = GetSpawnpoint();

			if ( spawnpoint.HasValue )
			{
				Transform = spawnpoint.Value;
			}

			Game.Current?.PlayerRespawned( this );

			LifeState = LifeState.Alive;
			Health = 100f;
			Velocity = Vector3.Zero;
			WaterLevel = 0f;

			CreateHull();
			ResetInterpolation();
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
			if ( !VoxelWorld.Current.IsValid() ) return;

			if ( IsServer )
			{
				if ( Input.Down( InputButton.Attack1 ) && NextBlockPlace )
				{
					if ( Input.Down( InputButton.Run ) )
					{
						var environmentLight = Entity.All.OfType<EnvironmentLightEntity>().FirstOrDefault();
						environmentLight.Color = Color.Red;
						environmentLight.SkyColor = Color.Red;
					}
					else
					{
						var container = HotbarInventory.Container;
						var item = container.GetFromSlot( CurrentHotbarIndex );

						if ( item.IsValid() && item is BlockItem blockItem )
						{
							var success = VoxelWorld.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, blockItem.BlockId, true );

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

					NextBlockPlace = 0.1f;
				}
				else if ( Input.Down( InputButton.Attack2 ) && NextBlockPlace )
				{
					if ( Input.Down( InputButton.Run ) )
					{
						var environmentLight = Entity.All.OfType<EnvironmentLightEntity>().FirstOrDefault();
						environmentLight.Color = Color.Black;
						environmentLight.SkyColor = Color.Black;
					}
					else if ( VoxelWorld.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var blockPosition ) )
					{
						var voxel = VoxelWorld.Current.GetVoxel( blockPosition );

						if ( voxel.IsValid )
						{
							if ( VoxelWorld.Current.SetBlockInDirection( Input.Position, Input.Rotation.Forward, 0 ) )
							{
								TryGiveBlock( voxel.BlockId, 1 );
							}
						}
					}

					NextBlockPlace = 0.1f;
				}

				if ( Input.Pressed( InputButton.Flashlight ) )
				{
					VoxelWorld.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var position );

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
									if ( Input.Down( InputButton.Duck ) )
										VoxelWorld.Current.SetBlockOnServer( blockPosition, VoxelWorld.Current.FindBlockId<WaterBlock>(), 0 );
									else
										VoxelWorld.Current.SetBlockOnServer( blockPosition, 0, 0 );
								}
							}
						}
					}
				}
				else if ( Input.Pressed( InputButton.Drop ) )
				{
					if ( Controller is MoveController )
					{
						Controller = new FlyController();
					}
					else
					{
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
				if ( Input.Down( InputButton.Score ) )
					Backpack.Current?.Open();
				else
					Backpack.Current?.Close();
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

				UpdateHotbarSlotKeys();

				if ( IsServer )
				{
					var item = HotbarInventory.Container.GetFromSlot( CurrentHotbarIndex );

					if ( item is WeaponItem weaponItem )
						ActiveChild = weaponItem.Weapon;
					else
						ActiveChild = null;
				}
			}

			var currentMap = VoxelWorld.Current;

			if ( IsServer )
			{
				if ( Input.Released( InputButton.Use) )
				{
					byte blockId = currentMap.FindBlockId<WhiteTorchBlock>();
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
					DebugOverlay.ScreenText( 6, $"Biome: {VoxelWorld.Current.GetBiomeAt( position.x, position.y ).Name}", 0.1f );
				}
			}

			Projectiles.Simulate();

			SimulateActiveChild( client, ActiveChild );

			var viewer = Client.Components.Get<ChunkViewer>();
			if ( !viewer.IsValid() ) return;
			if ( viewer.IsInMapBounds() && !viewer.IsCurrentChunkReady ) return;

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
			TryGiveBlock( VoxelWorld.Current.FindBlockId<GrassBlock>(), 1000 );
			TryGiveBlock( VoxelWorld.Current.FindBlockId<WindowBlock>(), 500 );

			TryGiveWeapon( "weapon_boomer" );
			TryGiveAmmo( AmmoType.Explosive, 200 );
		}

		public virtual void CreateInventories()
		{
			var hotbar = new InventoryContainer( this );
			hotbar.SetSlotLimit( 8 );
			hotbar.AddConnection( Client );
			hotbar.OnItemTaken += OnHotbarItemTaken;
			hotbar.OnItemGiven += OnHotbarItemGiven;
			InventorySystem.Register( hotbar );

			HotbarInventory = new NetInventory( hotbar );

			var backpack = new InventoryContainer( this );
			backpack.SetSlotLimit( 24 );
			backpack.AddConnection( Client );
			backpack.OnItemTaken += OnBackpackItemTaken;
			backpack.OnItemGiven += OnBackpackItemGiven;
			InventorySystem.Register( backpack );

			BackpackInventory = new NetInventory( backpack );
		}

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				InventorySystem.Remove( HotbarInventory.Container, true );
				InventorySystem.Remove( BackpackInventory.Container, true );
			}

			base.OnDestroy();
		}

		private void OnBackpackItemGiven( ushort slot, InventoryItem instance )
		{

		}

		private void OnBackpackItemTaken( ushort slot, InventoryItem instance )
		{

		}

		private void OnHotbarItemGiven( ushort slot, InventoryItem instance )
		{
			if ( instance is WeaponItem weapon )
			{
				if ( !weapon.Weapon.IsValid() )
				{
					weapon.Weapon = Library.Create<Weapon>( weapon.WeaponName );
					weapon.Weapon.OnCarryStart( this );
				}
			}
		}

		private void OnHotbarItemTaken( ushort slot, InventoryItem instance )
		{
			if ( instance is WeaponItem weapon )
			{
				if ( weapon.Weapon.IsValid() && instance.Container != HotbarInventory.Container )
				{
					weapon.Weapon.Delete();
					weapon.Weapon = null;
				}
			}
		}

		private void UpdateHotbarSlotKeys()
		{
			if ( Input.Pressed( InputButton.Slot0 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 1, HotbarInventory.Container.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot1 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 2, HotbarInventory.Container.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot2 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 3, HotbarInventory.Container.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot3 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 4, HotbarInventory.Container.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot4 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 5, HotbarInventory.Container.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot5 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 6, HotbarInventory.Container.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot6 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 7, HotbarInventory.Container.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot7 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 8, HotbarInventory.Container.SlotLimit - 1 );
		}
	}
}
