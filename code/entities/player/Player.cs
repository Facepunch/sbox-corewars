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
		public ProjectileSimulator Projectiles { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }
		public TimeUntil NextBlockPlace { get; private set; }

		private bool IsWaitingToRespawn { get; set; }

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

					item.Container.Remove( item.ItemId );
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
			var teams = Enum.GetValues( typeof( Team ) )
				.OfType<Team>()
				.Except( new Team[] { Team.None } )
				.ToList();

			var team = Rand.FromList( teams );

			SetTeam( team );
		}

		public void RespawnWhenAvailable()
		{
			IsWaitingToRespawn = true;
		}

		public virtual void Reset()
		{
			BackpackInventory.Instance.RemoveAll();
			HotbarInventory.Instance.RemoveAll();
			GiveInitialItems();
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
				Hotbar.Current?.SetContainer( HotbarInventory.Instance );
				Backpack.Current?.SetContainer( BackpackInventory.Instance );
			}

			base.ClientSpawn();
		}

		public override void Respawn()
		{
			Game.Current?.PlayerRespawned( this );

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
				if ( Input.Released( InputButton.Flashlight ) )
				{
					var tr = Trace.Ray( Input.Position, Input.Position + Input.Rotation.Forward * 500f )
						.WorldAndEntities()
						.Ignore( this )
						.Run();

					var item = InventorySystem.CreateItem<AmmoItem>();
					item.StackSize = 30;
					item.AmmoType = AmmoType.Explosive;

					var ent = new ItemEntity();
					ent.Position = tr.EndPosition;
					ent.SetItem( item );
				}

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
						var container = HotbarInventory.Instance;
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
					if ( VoxelWorld.Current.GetBlockInDirection( Input.Position, Input.Rotation.Forward, out var blockPosition ) )
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
			}

			var currentMap = VoxelWorld.Current;

			if ( IsClient  &&  currentMap.IsValid() )
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

			HotbarInventory = new NetInventoryContainer( hotbar );

			var backpack = new InventoryContainer( this );
			backpack.SetSlotLimit( 24 );
			backpack.AddConnection( Client );
			backpack.OnItemTaken += OnBackpackItemTaken;
			backpack.OnItemGiven += OnBackpackItemGiven;
			InventorySystem.Register( backpack );

			BackpackInventory = new NetInventoryContainer( backpack );
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
				if ( weapon.Weapon.IsValid() && instance.Container != HotbarInventory.Instance )
				{
					weapon.Weapon.Delete();
					weapon.Weapon = null;
				}
			}
		}

		private void UpdateHotbarSlotKeys()
		{
			if ( Input.Pressed( InputButton.Slot0 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 1, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot1 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 2, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot2 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 3, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot3 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 4, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot4 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 5, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot5 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 6, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot6 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 7, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot7 ) )
				CurrentHotbarIndex = (ushort)Math.Min( 8, HotbarInventory.Instance.SlotLimit - 1 );
		}
	}
}
