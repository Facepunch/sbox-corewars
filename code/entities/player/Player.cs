﻿using Facepunch.CoreWars.Inventory;
using Facepunch.Voxels;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Player : Sandbox.Player, IResettable, INameplate
	{
		[Net, Change( nameof( OnTeamChanged ) )] public Team Team { get; private set; }
		[Net] public IDictionary<StatModifier, float> Modifiers { get; private set; }
		[Net, Predicted] public ushort CurrentHotbarIndex { get; private set; }
		[Net, Predicted] public bool IsOutOfBreath { get; set; }
		[Net, Predicted] public float Stamina { get; set; }
		[Net] public NetInventoryContainer BackpackInventory { get; private set; }
		[Net] public NetInventoryContainer HotbarInventory { get; private set; }
		[Net] public NetInventoryContainer ChestInventory { get; private set; }
		[Net] public NetInventoryContainer EquipmentInventory { get; private set; }
		[Net] public TeamCore Core { get; private set; }
		[Net, Change] public IList<BaseBuff> Buffs { get; private set; }
		[Net] public IDictionary<string,int> Resources { get; private set; }

		public RealTimeSince TimeSinceLastHit { get; private set; }
		public Dictionary<ArmorSlot,List<BaseClothing>> Armor { get; private set; }
		public ProjectileSimulator Projectiles { get; private set; }
		public RealTimeUntil TimeUntilRespawn { get; private set; }
		public DamageInfo LastDamageTaken { get; private set; }
		public TimeUntil NextActionTime { get; private set; }
		public TimeSince LastPickupTime { get; private set; }
		public string DisplayName => Client.Name;

		public bool IsFriendly
		{
			get
			{
				if ( Local.Pawn is Player player )
				{
					return player.Team == Team;
				}

				return false;
			}
		}

		private TimeSince TimeSinceLastFootstep { get; set; }
		private TimeSince TimeSinceBackpackOpen { get; set; }
		private bool IsBackpackToggleMode { get; set; }
		private bool IsWaitingToRespawn { get; set; }
		private bool ShouldResetEyeRotation { get; set; }
		private BlockGhost BlockGhost { get; set; }
		private Nameplate Nameplate { get; set; }

		public Player() : base()
		{
			Projectiles = new( this );
		}

		[ConCmd.Server]
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

		[ConCmd.Server( "cw_give_item" )]
		public static void GiveItemCmd( string itemName, int amount )
		{
			if ( ConsoleSystem.Caller.Pawn is not Player player )
				return;

			var item = InventorySystem.CreateItem( itemName );
			item.StackSize = (ushort)amount;

			player.TryGiveItem( item );
		}

		[ConCmd.Server]
		public static void BuyUpgradeCmd( int index, string type )
		{
			if ( ConsoleSystem.Caller.Pawn is not Player player )
				return;

			var entity = FindByIndex( index );
			if ( entity is not TeamUpgradesEntity npc ) return;
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

		[ConCmd.Server]
		public static void BuyItemCmd( int index, string type )
		{
			if ( ConsoleSystem.Caller.Pawn is not Player player )
				return;

			var entity = FindByIndex( index );
			if ( entity is not IItemStore store ) return;
			if ( store.Position.Distance( player.Position ) > store.MaxUseDistance ) return;

			var item = store.Items.FirstOrDefault( i => i.GetType().Name == type );
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
			Modifiers = new Dictionary<StatModifier, float>();
			Resources = new Dictionary<string, int>();
			Armor = new();
			Buffs = new List<BaseBuff>();
		}

		public int GetResourceCount( Type type )
		{
			if ( Resources.TryGetValue( type.Name, out var count ) )
			{
				return count;
			}

			return 0;
		}

		public int GetResourceCount<T>() where T : ResourceItem
		{
			return GetResourceCount( typeof( T ) );
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

		public void GiveBuff( BaseBuff buff )
		{
			var existing = Buffs.Where( b => b.GetType() == buff.GetType() ).FirstOrDefault();

			if ( existing != null )
			{
				existing.TimeUntilExpired = existing.Duration;
				return;
			}

			buff.TimeUntilExpired = buff.Duration;
			buff.OnActivated( this );
			Buffs.Add( buff );
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

		public void AddModifier( StatModifier modifier, float value )
		{
			if ( Modifiers.TryGetValue( modifier, out var current ) )
			{
				value = current + value;
			}

			Modifiers[modifier] = value;
		}

		public void TakeModifier( StatModifier modifier, float value )
		{
			if ( Modifiers.TryGetValue( modifier, out var current ) )
			{
				value = current - value;
				Modifiers[modifier] = value;
			}
		}

		public float GetModifier( StatModifier modifier )
		{
			if ( Modifiers.TryGetValue( modifier, out var value ) )
			{
				return 1f + value;
			}

			return 1f;
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

		public void ReduceStamina( float amount )
		{
			Stamina = Math.Max( Stamina - amount, 0f );
		}

		public void GainStamina( float amount )
		{
			Stamina = Math.Min( Stamina + amount, 100f );
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

		public bool IsCoreValid()
		{
			var core = Team.GetCore();
			return core.IsValid() && core.LifeState == LifeState.Alive;
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
			Core = team.GetCore();

			OnTeamChanged( team );
		}

		public void AssignRandomTeam( bool assignToSmallestTeam = false )
		{
			var teams = Game.GetValidTeams().ToArray();

			if ( teams.Length == 0 )
			{
				SetTeam( Team.None );
				return;
			}

			Team team;

			if ( assignToSmallestTeam )
				team = teams.OrderBy( t => t.GetPlayers().Count() ).First();
			else
				team = Rand.FromArray( teams );

			SetTeam( team );
		}

		public void OnBuffsChanged( IList<BaseBuff> oldBuffs, IList<BaseBuff> newBuffs )
		{
			foreach ( var buff in oldBuffs )
			{
				if ( !newBuffs.Contains( buff ) && buff != null )
				{
					buff.OnExpired( this );
				}
			}

			foreach ( var buff in newBuffs )
			{
				if ( !oldBuffs.Contains( buff ) && buff != null )
				{
					buff.OnActivated( this );
				}
			}
		}

		[ClientRpc]
		public void ShowHitMarker( int hitboxGroup )
		{
			if ( hitboxGroup == 1 )
				Sound.FromScreen( "hitmarker.headshot" );
			else
				Sound.FromScreen( "hitmarker.hit" );

			TimeSinceLastHit = 0f;
		}

		public BlockType GetBlockBelow()
		{
			var world = VoxelWorld.Current;
			return world.GetBlockType( world.ToVoxelPosition( Position ) + Chunk.BlockDirections[1] );
		}

		public void RespawnWhenAvailable( float timeToRespawn = 0f )
		{
			IsWaitingToRespawn = true;
			TimeUntilRespawn = timeToRespawn;
		}

		public void RemoveBuff<T>() where T : BaseBuff
		{
			for ( var i = Buffs.Count - 1; i >= 0; i-- )
			{
				var buff = Buffs[i];

				if ( buff is T )
				{
					buff.OnExpired( this );
					Buffs.RemoveAt( i );
				}
			}
		}

		public void ClearBuffs()
		{
			foreach ( var buff in Buffs )
			{
				buff.OnExpired( this );
			}

			Buffs.Clear();
		}

		public virtual void RenderHud( Vector2 screenSize )
		{
			if ( ActiveChild is Weapon weapon && weapon.IsValid() )
			{
				weapon.RenderHud( screenSize );
			}
			else
			{
				var draw = Render.Draw2D;
				draw.BlendMode = BlendMode.Lighten;
				draw.Color = Color.Yellow.WithAlpha( 0.6f );
				draw.Circle( screenSize * 0.5f, 6f );
			}
		}

		public virtual void ClearInventories()
		{
			EquipmentInventory.Instance.RemoveAll();
			BackpackInventory.Instance.RemoveAll();
			HotbarInventory.Instance.RemoveAll();
			ChestInventory.Instance.RemoveAll();
		}

		public virtual void Reset()
		{
			Client.SetInt( "cores", 0 );
			Client.SetInt( "kills", 0 );
			ClearInventories();
			ClearBuffs();
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

				return new Transform( Rand.FromList( world.Spawnpoints ) );
			}

			var teamSpawnpoints = spawnpoints.Where( s => s.Team == Team ).ToList();

			PlayerSpawnpoint spawnpoint = null;

			if ( teamSpawnpoints.Count == 0 )
			{
				var lobbySpawnpoints = teamSpawnpoints.Where( s => s.Team == Team.None ).ToList();

				if ( lobbySpawnpoints.Count > 0 )
					spawnpoint = Rand.FromList( lobbySpawnpoints );
				else
					spawnpoint = Rand.FromList( spawnpoints );
			}
			else
			{
				spawnpoint = Rand.FromList( teamSpawnpoints );
			}

			if ( !spawnpoint.IsValid() )
			{
				return null;
			}

			return spawnpoint.Transform;
		}

		public virtual void OnMapLoaded()
		{
			EnableHideInFirstPerson = true;
			EnableAllCollisions = true;

			CameraMode = new FirstPersonCamera();
			Animator = new PlayerAnimator();
		}

		public override void Spawn()
		{
			EnableTouchPersists = true;
			EnableDrawing = false;
			EnableTouch = true;

			SetModel( "models/citizen/citizen.vmdl" );
			AttachClothing( "models/citizen_clothes/shoes/slippers/models/slippers.vmdl" );
			SetMaterialGroup( Rand.Int( MaterialGroupCount - 1 ) );

			base.Spawn();
		}

		public override void OnAnimEventFootstep( Vector3 position, int foot, float volume )
		{
			if ( LifeState == LifeState.Dead || !IsClient )
				return;

			if ( TimeSinceLastFootstep < 0.2f )
				return;

			var block = GetBlockBelow();

			if ( block is not AirBlock )
			{
				var sound = foot == 0 ? block.FootLeftSound : block.FootRightSound;

				if ( !string.IsNullOrEmpty( sound ) )
				{
					Sound.FromWorld( sound, position ).SetVolume( volume );
					return;
				}
			}

			volume *= FootstepVolume();

			TimeSinceLastFootstep = 0f;

			var trace = Trace.Ray( position, position + Vector3.Down * 20f )
				.Radius( 1 )
				.Ignore( this )
				.Run();

			if ( !trace.Hit ) return;

			trace.Surface.DoFootstep( this, trace, foot, volume );
		}

		public override float FootstepVolume()
		{
			return Velocity.WithZ( 0f ).Length.LerpInverse( 0f, 200f ) * 0.3f;
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

				Hud.AddKillFeed( To.Everyone, attacker, this, LastDamageTaken.Weapon, LastDamageTaken.Flags );
			}
			else
			{
				Hud.AddKillFeed( To.Everyone, this, LastDamageTaken.Flags );
			}

			BecomeRagdollOnClient( LastDamageTaken.Force, LastDamageTaken.BoneIndex );

			EnableAllCollisions = false;
			EnableDrawing = false;
			Controller = null;
			CameraMode = new SpectateCamera();

			RespawnScreen.Show( To.Single( this ), 5f, LastDamageTaken.Attacker, LastDamageTaken.Weapon );

			RespawnWhenAvailable( 5f );
			ClearBuffs();

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

			var weapons = Children.OfType<Weapon>().ToArray();

			foreach ( var weapon in weapons )
			{
				weapon.Delete();
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

			Nameplate = new Nameplate( this );

			TeamList.Refresh();

			base.ClientSpawn();
		}

		public override void Respawn()
		{
			var isLobbyState = Game.IsState<LobbyState>();

			RespawnScreen.Hide( To.Single( this ) );

			if ( !isLobbyState && !IsCoreValid() )
			{
				EnableAllCollisions = false;
				EnableDrawing = false;
				Controller = new FlyController
				{
					EnableCollisions = false
				};
				LifeState = LifeState.Dead;
				Velocity = Vector3.Zero;

				ClearInventories();
			}
			else
			{
				Game.Current?.PlayerRespawned( this );

				EnableAllCollisions = true;
				ResetEyeRotation( To.Single( this ) );
				EnableDrawing = true;
				LifeState = LifeState.Alive;
				Controller = new MoveController
				{
					WalkSpeed = 200f,
					SprintSpeed = 325f
				};
				Stamina = 100f;
				Health = 100f;
				Velocity = Vector3.Zero;
				WaterLevel = 0f;

				CreateHull();

				if ( isLobbyState )
					ClearInventories();
				else
					GiveInitialItems();
			}

			CameraMode = new FirstPersonCamera();

			ClearBuffs();

			var spawnpoint = GetSpawnpoint();

			if ( spawnpoint.HasValue )
			{
				Transform = spawnpoint.Value;
			}

			InitializeHotbarWeapons();
			ResetInterpolation();
		}

		public override void BuildInput( InputBuilder input )
		{
			if ( ShouldResetEyeRotation )
			{
				input.ViewAngles = Angles.Zero;
				ShouldResetEyeRotation = false;
			}

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

			if ( Game.IsState<GameState>() )
			{
				if ( LifeState == LifeState.Alive )
				{
					SimulateGameState( client );
				}
			}

			var viewer = Client.Components.Get<ChunkViewer>();

			if ( !viewer.IsValid() )
				return;

			if ( viewer.IsInWorld() && !viewer.IsCurrentChunkReady )
				return;

			if ( IsServer && viewer.IsBelowWorld() )
			{
				if ( LifeState == LifeState.Alive )
				{
					var damageInfo = DamageInfo.Generic( 1000f )
						.WithPosition( Position )
						.WithFlag( DamageFlags.Fall );

					TakeDamage( damageInfo );
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
			if ( info.Attacker is Player attacker )
			{
				if ( attacker == this && ( info.Flags.HasFlag( DamageFlags.Fall ) || info.Flags.HasFlag( DamageFlags.Generic ) ) )
				{
					RemoveBuff<StealthBuff>();
					LastDamageTaken = info;
					base.TakeDamage( info );
					return;
				}

				if ( !Game.FriendlyFire && attacker.Team == Team )
					return;

				info.Damage *= attacker.GetModifier( StatModifier.Damage );

				if ( attacker.Core.IsValid() )
				{
					var damageTier = attacker.Core.GetUpgradeTier( "damage" );

					if ( damageTier >= 2 )
						info.Damage *= 1.35f;
					else if ( damageTier >= 1 )
						info.Damage *= 1.2f;
				}

				if ( Core.IsValid() )
				{
					var armorTier = Core.GetUpgradeTier( "armor" );

					if ( armorTier >= 3 )
						info.Damage *= 0.4f;
					else if ( armorTier >= 2 )
						info.Damage *= 0.6f;
					else if ( armorTier >= 1 )
						info.Damage *= 0.8f;
				}

				using ( Prediction.Off() )
				{
					var particles = Particles.Create( "particles/gameplay/player/taken_damage/taken_damage.vpcf", info.Position );
					particles.SetForward( 0, info.Force.Normal );
				}

				var hitboxGroup = GetHitboxGroup( info.HitboxIndex );
				attacker.ShowHitMarker( To.Single( attacker ), hitboxGroup );

				FloatingDamage.Show( this, info.Damage, info.Position );
				RemoveBuff<StealthBuff>();

				if ( info.Flags.HasFlag( DamageFlags.Blunt ) )
				{
					ApplyAbsoluteImpulse( info.Force );
				}
			}

			LastDamageTaken = info;
			base.TakeDamage( info );
		}

		protected virtual void SimulateBlockGhost( Client client )
		{
			var world = VoxelWorld.Current;
			var container = HotbarInventory.Instance;
			var blockItem = container.GetFromSlot( CurrentHotbarIndex ) as BlockItem;

			if ( blockItem.IsValid() )
			{
				var position = GetBlockPosition( Input.Position, Input.Rotation.Forward );
				var ghost = GetOrCreateBlockGhost();

				if ( position.HasValue )
				{
					var block = world.GetBlockType( position.Value );

					if ( block is AirBlock )
					{
						ghost.EnableDrawing = true;
						ghost.Position = world.ToSourcePosition( position.Value );
					}
					else
					{
						ghost.EnableDrawing = false;
					}
				}
				else
				{
					ghost.EnableDrawing = false;
				}
			}
			else
			{
				DestroyBlockGhost();
			}
		}

		protected virtual void TryPlaceBlockItem( Client client, BlockItem item, Vector3 eyePosition, Vector3 direction )
		{
			var position = GetBlockPosition( Input.Position, Input.Rotation.Forward );
			if ( !position.HasValue ) return;

			var world = VoxelWorld.Current;
			var block = world.GetBlockType( position.Value );
			if ( block is not AirBlock ) return;

			var bbox = world.ToSourceBBox( position.Value );

			if ( !FindInBox( bbox ).Any() )
			{
				var sourcePosition = world.ToSourcePosition( position.Value );

				if ( CanBuildAt( sourcePosition ) )
				{
					item.StackSize--;

					world.SetBlockOnServer( position.Value, item.BlockId );

					using ( Prediction.Off() )
					{
						var particles = Particles.Create( "particles/gameplay/blocks/block_placed/block_placed.vpcf" );
						particles.SetPosition( 0, world.ToSourcePositionCenter( position.Value ) );
						particles.SetPosition( 6, Team.GetColor() );
						PlaySound( "block.place" );
					}

					if ( item.StackSize <= 0 )
					{
						InventorySystem.RemoveItem( item );
					}
				}
			}
		}

		protected virtual void SimulateGameState( Client client )
		{
			if ( Stamina <= 10f )
				IsOutOfBreath = true;
			else if ( IsOutOfBreath && Stamina >= 25f )
				IsOutOfBreath = false;

			if ( IsClient )
			{
				SimulateBlockGhost( client );
			}

			if ( IsServer )
			{
				if ( Input.Pressed( InputButton.PrimaryAttack ) && ActiveChild is Weapon )
				{
					RemoveBuff<StealthBuff>();
				}

				if ( Input.Released( InputButton.PrimaryAttack ) && NextActionTime )
				{
					var container = HotbarInventory.Instance;
					var item = container.GetFromSlot( CurrentHotbarIndex );

					if ( item.IsValid() )
					{
						if ( item is BlockItem blockItem )
						{
							TryPlaceBlockItem( client, blockItem, Input.Position, Input.Rotation.Forward );
						}
						else if ( item is IConsumableItem consumable )
						{
							consumable.Consume( this );
						}
					}

					NextActionTime = 0.1f;
				}

				if ( Input.Released( InputButton.Drop ) )
				{
					var container = HotbarInventory.Instance;
					var item = container.GetFromSlot( CurrentHotbarIndex );

					if ( item.IsValid() && item.CanBeDropped )
					{
						var entity = new ItemEntity();
						entity.Position = Input.Position + Input.Rotation.Forward * 32f;
						entity.SetItem( item );
						entity.ApplyLocalImpulse( Input.Rotation.Forward * 100f + Vector3.Up * 50f );
					}

					PlaySound( "item.dropped" );
				}
			}
			else if ( Prediction.FirstTime )
			{
				if ( Input.Pressed( InputButton.Score ) )
				{
					if ( !Backpack.Current.IsOpen )
						TimeSinceBackpackOpen = 0f;
					else
						IsBackpackToggleMode = false;

					if ( IDialog.IsActive() )
						IDialog.CloseActive();
					else
						Backpack.Current?.Open();
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

				if ( Input.Down( InputButton.Menu ) )
					TeamList.Open();
				else
					TeamList.Close();
			}

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

			var hotbarItem = HotbarInventory.Instance.GetFromSlot( CurrentHotbarIndex );

			if ( hotbarItem is WeaponItem weaponItem )
				ActiveChild = weaponItem.Weapon;
			else
				ActiveChild = null;

			if ( IsClient && Prediction.FirstTime )
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

			Projectiles.Simulate();

			SimulateActiveChild( client, ActiveChild );
		}

		protected virtual void OnTeamChanged( Team team )
		{
			if ( IsClient )
			{
				TeamList.Refresh();
			}
		}

		protected virtual void GiveInitialItems()
		{
			var swords = FindItems<SwordItemTier1>();

			if ( !swords.Any() )
			{
				var sword = InventorySystem.CreateItem<SwordItemTier1>();
				TryGiveItem( sword );
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

		public override void Touch( Entity other )
		{
			base.Touch( other );

			if ( IsClient ) return;

			if ( LifeState == LifeState.Dead )
				return;

			if ( other is not ItemEntity itemEntity )
				return;

			if ( !itemEntity.TimeUntilCanPickup || !itemEntity.Item.IsValid() )
				return;

			var remaining = TryGiveItem( itemEntity.Item.Instance );

			if ( remaining == 0 )
			{
				if ( LastPickupTime > 1f )
				{
					var effect = Particles.Create( "particles/gameplay/items/item_pick_up/item_pick_up.vpcf", this );
					effect.SetEntity( 0, this );
				}

				LastPickupTime = 0f;
				PlaySound( "item.pickup" );
				itemEntity.Take();
			}
		}

		[ClientRpc]
		protected void ResetEyeRotation()
		{
			ShouldResetEyeRotation = true;
		}

		[Event.Tick.Server]
		protected virtual void ServerTick()
		{
			if ( IsWaitingToRespawn && TimeUntilRespawn )
			{
				var spawnpoint = GetSpawnpoint();
				if ( !spawnpoint.HasValue ) return;
				IsWaitingToRespawn = false;
				Respawn();
			}

			for ( var i = Buffs.Count - 1; i >= 0; i-- )
			{
				var buff = Buffs[i];

				if ( buff.TimeUntilExpired )
				{
					buff.OnExpired( this );
					Buffs.RemoveAt( i );
				}
			}

			UpdateResourceCount<IronItem>();
			UpdateResourceCount<GoldItem>();
			UpdateResourceCount<CrystalItem>();
		}

		protected void UpdateResourceCount<T>() where T : ResourceItem
		{
			Resources[ typeof(T).Name ] = FindItems<T>().Sum( i => (int)i.StackSize );
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

		private BlockGhost GetOrCreateBlockGhost()
		{
			if ( BlockGhost.IsValid() ) return BlockGhost;
			BlockGhost = new();
			return BlockGhost;
		}

		private void DestroyBlockGhost()
		{
			if ( !BlockGhost.IsValid() ) return;
			BlockGhost.Delete();
			BlockGhost = null;
		}

		private IntVector3? GetBlockPosition( Vector3 eyePosition, Vector3 direction )
		{
			var world = VoxelWorld.Current;
			var range = 3.5f;
			var face = world.Trace( eyePosition * (1.0f / world.VoxelSize), direction, range, out var position, out var _ );

			if ( face == BlockFace.Invalid )
			{
				var belowPosition = world.ToVoxelPosition( Position ) + Chunk.BlockDirections[1];
				var belowBlock = world.GetBlockType( belowPosition );

				if ( belowBlock.BlockId > 0 )
				{
					var currentPosition = belowPosition;

					for ( var i = 0; i < range.FloorToInt(); i++ )
					{
						var neighborPosition = currentPosition;

						if ( direction.x < -0.25f )
							neighborPosition -= Chunk.BlockDirections[(int)BlockFace.North];
						else if ( direction.x > 0.25f )
							neighborPosition -= Chunk.BlockDirections[(int)BlockFace.South];

						if ( direction.y < -0.25f )
							neighborPosition -= Chunk.BlockDirections[(int)BlockFace.East];
						else if ( direction.y > 0.25f )
							neighborPosition -= Chunk.BlockDirections[(int)BlockFace.West];

						var neighborBlock = world.GetBlockType( neighborPosition );

						if ( neighborBlock is AirBlock )
							return neighborPosition;

						currentPosition = neighborPosition;
					}
				}

				return null;
			}
			else
			{
				var adjacentPosition = position + Chunk.BlockDirections[(int)face];
				return adjacentPosition;
			}
		}

		private void AddClothingToArmorSlot( ArmorSlot slot, BaseClothing clothing )
		{
			if ( !Armor.TryGetValue( slot, out var models ) )
			{
				models = new List<BaseClothing>();
				Armor[slot] = models;
			}

			models.Add( clothing );
		}

		private void OnEquipmentItemGiven( ushort slot, InventoryItem instance )
		{
			if ( instance is ArmorItem armor )
			{
				if ( Armor.TryGetValue( armor.ArmorSlot, out var models ) )
				{
					foreach ( var model in models )
					{
						model.Delete();
					}

					Armor.Remove( armor.ArmorSlot );
				}

				if ( !string.IsNullOrEmpty( armor.PrimaryModel ) )
				{
					AddClothingToArmorSlot( armor.ArmorSlot, AttachClothing( armor.PrimaryModel ) );
				}

				if ( !string.IsNullOrEmpty( armor.SecondaryModel ) )
				{
					AddClothingToArmorSlot( armor.ArmorSlot, AttachClothing( armor.SecondaryModel ) );
				}
			}
		}

		private void OnEquipmentItemTaken( ushort slot, InventoryItem instance )
		{
			if ( instance is ArmorItem armor && !EquipmentInventory.Is( instance.Container ) )
			{
				if ( Armor.TryGetValue( armor.ArmorSlot, out var models ) )
				{
					foreach ( var model in models )
					{
						model.Delete();
					}

					Armor.Remove( armor.ArmorSlot );
				}
			}
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
				InitializeWeaponItem( weapon );
			}
		}

		private void OnHotbarItemTaken( ushort slot, InventoryItem instance )
		{
			if ( instance is WeaponItem weapon )
			{
				if ( weapon.Weapon.IsValid() && !HotbarInventory.Is( instance.Container ) )
				{
					weapon.Weapon.Delete();
					weapon.Weapon = null;
					weapon.IsDirty = true;
				}
			}
		}

		private void InitializeHotbarWeapons()
		{
			foreach ( var item in HotbarInventory.Instance.ItemList )
			{
				if ( item is WeaponItem weapon )
				{
					InitializeWeaponItem( weapon );
				}
			}
		}

		private void InitializeWeaponItem( WeaponItem item )
		{
			if ( !item.Weapon.IsValid() )
			{
				try
				{
					item.Weapon = TypeLibrary.Create<Weapon>( item.WeaponName );
					item.Weapon.SetWeaponItem( item );
					item.Weapon.OnCarryStart( this );
					item.IsDirty = true;
				}
				catch ( Exception e )
				{
					Log.Error( e );
				}
			}
		}

		private void UpdateHotbarSlotKeys()
		{
			var index = CurrentHotbarIndex;

			if ( Input.Pressed( InputButton.Slot1 ) )
				index = (ushort)Math.Min( 0, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot2 ) )
				index = (ushort)Math.Min( 1, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot3 ) )
				index = (ushort)Math.Min( 2, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot4 ) )
				index = (ushort)Math.Min( 3, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot5 ) )
				index = (ushort)Math.Min( 4, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot6 ) )
				index = (ushort)Math.Min( 5, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot7 ) )
				index = (ushort)Math.Min( 6, HotbarInventory.Instance.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot8 ) )
				index = (ushort)Math.Min( 7, HotbarInventory.Instance.SlotLimit - 1 );

			if ( index != CurrentHotbarIndex )
			{
				var container = HotbarInventory.Instance;
				var item = container.GetFromSlot( index );

				if ( item is IConsumableItem consumable )
				{
					if ( IsServer )
					{
						consumable.Consume( this );
					}

					return;
				}

				CurrentHotbarIndex = index;
			}
		}
	}
}
