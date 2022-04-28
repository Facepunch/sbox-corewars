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
	public partial class Player : Sandbox.Player, IResettable
	{
		[Net, Change( nameof( OnTeamChanged ) )] public Team Team { get; private set; }
		[Net, Predicted] public ushort CurrentHotbarIndex { get; private set; }
		[Net] public NetInventoryContainer BackpackInventory { get; private set; }
		[Net] public NetInventoryContainer HotbarInventory { get; private set; }
		[Net] public NetInventoryContainer ChestInventory { get; private set; }
		[Net] public NetInventoryContainer EquipmentInventory { get; private set; }
		public ProjectileSimulator Projectiles { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }
		public TimeUntil NextBlockPlace { get; private set; }

		private TimeSince TimeSinceBackpackOpen { get; set; }
		private bool IsBackpackToggleMode { get; set; }
		private bool IsWaitingToRespawn { get; set; }

		public Player() : base()
		{
			Projectiles = new( this );
		}

		[ServerCmd]
		public static void UseEntityCmd( int index )
		{
			if ( ConsoleSystem.Caller.Pawn is not Player player )
				return;

			var entity = FindByIndex( index );

			if ( entity is IUsable usable && usable.IsUsable( player ) )
			{
				if ( entity.Position.Distance( player.Position ) <= usable.MaxUseDistance )
				{
					usable.OnUsed( player );
				}
			}
		}

		[ServerCmd]
		public static void BuyUpgradeCmd( int index, string type )
		{
			if ( ConsoleSystem.Caller.Pawn is not Player player )
				return;

			var entity = FindByIndex( index );
			if ( entity is not TeamUpgradesNPC npc ) return;
			if ( npc.Position.Distance( player.Position ) > npc.MaxUseDistance ) return;

			var item = npc.Upgrades.FirstOrDefault( i => i.GetType().Name == type );
			if ( item == null ) return;
			if ( !item.CanAfford( player ) ) return;
			if ( !item.CanPurchase( player ) ) return;

			foreach ( var kv in item.Costs )
			{
				player.TakeResources( kv.Key, kv.Value );
			}

			item.OnPurchased( player );
		}

		[ServerCmd]
		public static void BuyItemCmd( int index, string type )
		{
			if ( ConsoleSystem.Caller.Pawn is not Player player )
				return;

			var entity = FindByIndex( index );
			if ( entity is not ItemStoreNPC npc ) return;
			if ( npc.Position.Distance( player.Position ) > npc.MaxUseDistance ) return;

			var item = npc.Items.FirstOrDefault( i => i.GetType().Name == type );
			if ( item == null ) return;
			if ( !item.CanAfford( player ) ) return;
			if ( !item.CanPurchase( player ) ) return;

			foreach ( var kv in item.Costs )
			{
				player.TakeResources( kv.Key, kv.Value );
			}

			item.OnPurchased( player );
		}

		public Player( Client client ) : this()
		{
			CurrentHotbarIndex = 0;
			client.Pawn = this;
			CreateInventories();
		}

		public bool TryGiveWeapon<T>() where T : WeaponItem
		{
			var item = InventorySystem.CreateItem<T>();
			
			if ( HotbarInventory.Instance.Give( item ) )
				return true;

			return BackpackInventory.Instance.Give( item );
		}

		public void TryGiveAmmo( AmmoType type, ushort amount )
		{
			var item = InventorySystem.CreateItem<AmmoItem>();
			item.AmmoType = type;
			item.StackSize = amount;

			var remaining = HotbarInventory.Instance.Stack( item );

			if ( remaining > 0 )
			{
				BackpackInventory.Instance.Stack( item );
			}
		}

		public void TryGiveBlock( byte blockId, ushort amount )
		{
			var item = InventorySystem.CreateItem<BlockItem>();
			item.BlockId = blockId;
			item.StackSize = amount;

			var remaining = HotbarInventory.Instance.Stack( item );

			if ( remaining > 0 )
			{
				BackpackInventory.Instance.Stack( item );
			}
		}

		public bool TryGiveArmor( ArmorItem item )
		{
			var slotToIndex = (int)item.ArmorSlot - 1;
			return EquipmentInventory.Instance.Give( item, (ushort)slotToIndex );
		}

		public ushort TryGiveItem( InventoryItem item )
		{
			var remaining = HotbarInventory.Instance.Stack( item );

			if ( remaining > 0 )
			{
				remaining = BackpackInventory.Instance.Stack( item );
			}

			return remaining;
		}

		public bool CanBuildAt( Vector3 position )
		{
			var positionToBBox = new BBox( position );

			foreach ( var trigger in All.OfType<BuildExclusionZone>() )
			{
				if ( trigger.WorldSpaceBounds.Contains( positionToBBox ) )
				{
					return false;
				}
			}

			return true;
		}

		public List<T> FindItems<T>() where T : InventoryItem
		{
			var items = new List<T>();
			items.AddRange( HotbarInventory.Instance.FindItems<T>() );
			items.AddRange( BackpackInventory.Instance.FindItems<T>() );
			items.AddRange( EquipmentInventory.Instance.FindItems<T>() );
			return items;
		}

		public List<InventoryItem> FindItems( Type type )
		{
			var items = new List<InventoryItem>();
			items.AddRange( HotbarInventory.Instance.FindItems( type ) );
			items.AddRange( BackpackInventory.Instance.FindItems( type ) );
			items.AddRange( EquipmentInventory.Instance.FindItems( type ) );
			return items;
		}

		public int TakeResources( Type type, int count )
		{
			var items = new List<ResourceItem>();

			items.AddRange( HotbarInventory.Instance.FindItems<ResourceItem>() );
			items.AddRange( BackpackInventory.Instance.FindItems<ResourceItem>() );

			var amountLeftToTake = count;
			var totalAmountTaken = 0;

			for ( int i = items.Count - 1; i >= 0; i-- )
			{
				var item = items[i];

				if ( item.GetType() == type )
				{
					if ( item.StackSize >= amountLeftToTake )
					{
						totalAmountTaken += amountLeftToTake;
						item.StackSize -= (ushort)amountLeftToTake;

						if ( item.StackSize > 0 )
							return totalAmountTaken;
					}
					else
					{
						amountLeftToTake -= item.StackSize;
						totalAmountTaken += item.StackSize;
						item.StackSize = 0;
					}

					item.Remove();
				}
			}

			return totalAmountTaken;
		}

		public ushort TakeAmmo( AmmoType type, ushort count )
		{
			var items = new List<AmmoItem>();

			items.AddRange( HotbarInventory.Instance.FindItems<AmmoItem>() );
			items.AddRange( BackpackInventory.Instance.FindItems<AmmoItem>() );

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

					item.Remove();
				}
			}

			return totalAmountTaken;
		}

		public int GetAmmoCount( AmmoType type )
		{
			var items = new List<AmmoItem>();

			items.AddRange( HotbarInventory.Instance.FindItems<AmmoItem>() );
			items.AddRange( BackpackInventory.Instance.FindItems<AmmoItem>() );

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

		public void AssignRandomTeam()
		{
			var teams = Game.GetValidTeams().ToArray();
			var team = Rand.FromArray( teams );

			SetTeam( team );
		}

		public void RespawnWhenAvailable()
		{
			IsWaitingToRespawn = true;
		}

		public virtual void Reset()
		{
			EquipmentInventory.Instance.RemoveAll();
			BackpackInventory.Instance.RemoveAll();
			HotbarInventory.Instance.RemoveAll();
			ChestInventory.Instance.RemoveAll();
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

			CameraMode = new FirstPersonCamera();

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
			EnableDrawing = false;
			EnableTouch = true;

			base.Spawn();
		}

		public override void OnKilled()
		{
			if ( LastDamageTaken.Attacker is Player attacker )
			{
				var resources = HotbarInventory.Instance.FindItems<ResourceItem>();
				resources.AddRange( BackpackInventory.Instance.FindItems<ResourceItem>() );

				foreach ( var resource in resources )
				{
					resource.Container.Remove( resource );
					attacker.TryGiveItem( resource );
				}
			}

			EnableDrawing = false;

			RespawnWhenAvailable();

			var itemsToDrop = FindItems<InventoryItem>().Where( i => i.DropOnDeath );

			foreach ( var item in itemsToDrop )
			{
				var entity = new ItemEntity();
				entity.Position = WorldSpaceBounds.Center + Vector3.Up * 64f;
				entity.SetItem( item );
				entity.ApplyLocalImpulse( Vector3.Random * 100f );
			}

			var itemsToRemove = FindItems<InventoryItem>().Where( i => i.RemoveOnDeath );

			foreach ( var item in itemsToRemove )
			{
				item.Remove();
			}

			base.OnKilled();
		}

		public override void ClientSpawn()
		{
			if ( IsLocalPawn )
			{
				var backpack = BackpackInventory.Instance;
				var equipment = EquipmentInventory.Instance;
				var hotbar = HotbarInventory.Instance;

				backpack.SetTransferTargetHandler( GetBackpackTransferTarget );
				equipment.SetTransferTargetHandler( GetEquipmentTransferTarget );
				hotbar.SetTransferTargetHandler( GetHotbarTransferTarget );

				Backpack.Current?.SetBackpack( backpack );
				Backpack.Current?.SetEquipment( equipment );
				Backpack.Current?.SetHotbar( hotbar );

				Hotbar.Current?.SetContainer( hotbar );
			}

			base.ClientSpawn();
		}

		public override void Respawn()
		{
			Game.Current?.PlayerRespawned( this );

			EnableDrawing = true;
			LifeState = LifeState.Alive;
			Health = 100f;
			Velocity = Vector3.Zero;
			WaterLevel = 0f;

			CreateHull();

			var spawnpoint = GetSpawnpoint();

			if ( spawnpoint.HasValue )
			{
				Transform = spawnpoint.Value;
			}

			ResetInterpolation();
			GiveInitialItems();
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
			var world = VoxelWorld.Current;

			if ( !world.IsValid() ) return;

			if ( IsServer )
			{
				if ( Input.Down( InputButton.Attack1 ) && NextBlockPlace )
				{
					var container = HotbarInventory.Instance;
					var item = container.GetFromSlot( CurrentHotbarIndex );

					if ( item.IsValid() && item is BlockItem blockItem )
					{
						var success = world.SetBlockInDirection( Input.Position, Input.Rotation.Forward, blockItem.BlockId, true, 200f, ( position ) =>
						{
							var sourcePosition = world.ToSourcePosition( position );
							return CanBuildAt( sourcePosition );
						} );

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
					if ( world.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var blockPosition, 200f ) )
					{
						var voxel = world.GetVoxel( blockPosition );

						if ( voxel.IsValid )
						{
							if ( world.SetBlockInDirection( Input.Position, Input.Rotation.Forward, 0 ) )
							{
								TryGiveBlock( voxel.BlockId, 1 );
							}
						}
					}

					NextBlockPlace = 0.1f;
				}
			}
			else
			{
				if ( Input.Pressed( InputButton.Score ) )
				{
					if ( !Backpack.Current.IsOpen )
						TimeSinceBackpackOpen = 0f;
					else
						IsBackpackToggleMode = false;

					if ( !IDialog.IsActive() )
					{
						Backpack.Current?.Open();
					}
				}

				if ( Input.Released( InputButton.Score ) )
				{
					if ( TimeSinceBackpackOpen <= 0.2f )
					{
						IsBackpackToggleMode = true;
					}

					if ( !IsBackpackToggleMode )
					{
						Backpack.Current?.Close();
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

				var maxSlotIndex = HotbarInventory.Instance.SlotLimit - 1;

				if ( currentSlotIndex < 0 )
					currentSlotIndex = maxSlotIndex;
				else if ( currentSlotIndex > maxSlotIndex )
					currentSlotIndex = 0;


				CurrentHotbarIndex = (ushort)currentSlotIndex;

				UpdateHotbarSlotKeys();

				if ( IsServer )
				{
					var item = HotbarInventory.Instance.GetFromSlot( CurrentHotbarIndex );

					if ( item is WeaponItem weaponItem )
						ActiveChild = weaponItem.Weapon;
					else
						ActiveChild = null;
				}
				else
				{
					if ( Input.Released( InputButton.Use ) )
					{
						if ( !IDialog.IsActive() )
						{
							var trace = Trace.Ray( Input.Position, Input.Position + Input.Rotation.Forward * 10000f )
								.EntitiesOnly()
								.Ignore( this )
								.Ignore( ActiveChild )
								.Run();

							if ( trace.Entity is IUsable usable )
							{
								UseEntityCmd( trace.Entity.NetworkIdent );
							}
						}
						else
						{
							IDialog.CloseActive();
						}
					}
				}
			}

			if ( IsClient && world.IsValid() )
			{
				var position = world.ToVoxelPosition( Input.Position );
				var voxel = world.GetVoxel( position );

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
			if ( info.Attacker is Player attacker )
			{
				if ( Team != Team.None && attacker.Team == Team )
					return;
			}

			LastDamageTaken = info;

			base.TakeDamage( info );
		}

		protected virtual void OnTeamChanged( Team team )
		{

		}

		protected virtual void GiveInitialItems()
		{
			var crowbars = FindItems<CrowbarItemTier1>();

			if ( !crowbars.Any() )
			{
				var crowbar = InventorySystem.CreateItem<CrowbarItemTier1>();
				TryGiveItem( crowbar );
			}
		}

		public virtual void CreateInventories()
		{
			var hotbar = new InventoryContainer( this );
			hotbar.SetSlotLimit( 8 );
			hotbar.AddConnection( Client );
			hotbar.OnItemTaken += OnHotbarItemTaken;
			hotbar.OnItemGiven += OnHotbarItemGiven;
			InventorySystem.Register( hotbar );

			HotbarInventory = new NetInventoryContainer( hotbar );

			var backpack = new InventoryContainer( this );
			backpack.SetSlotLimit( 24 );
			backpack.AddConnection( Client );
			backpack.OnItemTaken += OnBackpackItemTaken;
			backpack.OnItemGiven += OnBackpackItemGiven;
			InventorySystem.Register( backpack );

			BackpackInventory = new NetInventoryContainer( backpack );

			var chest = new InventoryContainer( this );
			chest.SetSlotLimit( 24 );
			chest.AddConnection( Client );
			InventorySystem.Register( chest );

			ChestInventory = new NetInventoryContainer( chest );

			var equipment = new InventoryContainer( this );
			equipment.SetSlotLimit( 3 );
			equipment.AddConnection( Client );
			equipment.OnItemTaken += OnEquipmentItemTaken;
			equipment.OnItemGiven += OnEquipmentItemGiven;
			equipment.SetGiveCondition( CanGiveEquipmentItem );
			InventorySystem.Register( equipment );

			EquipmentInventory = new NetInventoryContainer( equipment );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is not ItemEntity itemEntity ) return;
			if ( !itemEntity.TimeUntilCanPickup || !itemEntity.Item.IsValid() ) return;

			var remaining = TryGiveItem( itemEntity.Item.Instance );

			if ( remaining == 0 )
			{
				itemEntity.Take();
			}

			base.StartTouch( other );
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( IsWaitingToRespawn )
			{
				var spawnpoint = GetSpawnpoint();
				if ( !spawnpoint.HasValue ) return;
				IsWaitingToRespawn = false;
				Respawn();
			}
		}

		protected override void OnDestroy()
		{
			if ( IsServer )
			{
				InventorySystem.Remove( HotbarInventory.Instance, true );
				InventorySystem.Remove( BackpackInventory.Instance, true );
				InventorySystem.Remove( EquipmentInventory.Instance, true );
				InventorySystem.Remove( ChestInventory.Instance, true );
			}

			base.OnDestroy();
		}

		private bool CanGiveEquipmentItem( ushort slot, InventoryItem item )
		{
			if ( item is not ArmorItem armor )
				return false;

			if ( armor.ArmorSlot == ArmorSlot.Head )
				return slot == 0;

			if ( armor.ArmorSlot == ArmorSlot.Chest )
				return slot == 1;

			if ( armor.ArmorSlot == ArmorSlot.Legs )
				return slot == 2;

			return false;
		}

		private InventoryContainer GetBackpackTransferTarget( InventoryItem item )
		{
			return Storage.Current.IsOpen ? Storage.Current.StorageContainer : HotbarInventory.Instance;
		}

		private InventoryContainer GetEquipmentTransferTarget( InventoryItem item )
		{
			return Storage.Current.IsOpen ? Storage.Current.StorageContainer : BackpackInventory.Instance;
		}

		private InventoryContainer GetHotbarTransferTarget( InventoryItem item )
		{
			return Storage.Current.IsOpen ? Storage.Current.StorageContainer : BackpackInventory.Instance;
		}

		private void OnEquipmentItemGiven( ushort slot, InventoryItem instance )
		{

		}

		private void OnEquipmentItemTaken( ushort slot, InventoryItem instance )
		{

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
					try
					{
						weapon.Weapon = Library.Create<Weapon>( weapon.WeaponName );
						weapon.Weapon.OnCarryStart( this );
					}
					catch ( Exception e )
					{
						Log.Error( e );
					}
				}
			}
		}

		private void OnHotbarItemTaken( ushort slot, InventoryItem instance )
		{
			if ( instance is WeaponItem weapon )
			{
				if ( weapon.Weapon.IsValid() && instance.Container != HotbarInventory.Instance )
				{
					weapon.Weapon.Delete();
					weapon.Weapon = null;
				}
			}
		}

		private void UpdateHotbarSlotKeys()
		{
			if ( Input.Pressed( InputButton.Slot1 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 0, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot2 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 1, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot3 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 2, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot4 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 3, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot5 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 4, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot6 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 5, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot7 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 6, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot8 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 7, HotbarInventory.Instance.SlotLimit - 1 );
		}
	}
}
