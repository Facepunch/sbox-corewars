using Facepunch.Voxels;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.CoreWars
{
	public partial class Player : BasePlayer, IResettable, INameplate
	{
		public static Player Me => Local.Pawn as Player;

		[Net, Change( nameof( OnTeamChanged ) )] public Team Team { get; private set; }
		[Net] public IDictionary<StatModifier, float> Modifiers { get; private set; }
		[Net, Predicted] public ushort HotbarIndex { get; private set; }
		[Net, Predicted] public bool IsOutOfBreath { get; set; }
		[Net, Predicted] public float Stamina { get; set; }

		[Net] private NetInventoryContainer InternalBackpack { get; set; }
		public InventoryContainer Backpack => InternalBackpack.Value;

		[Net] private NetInventoryContainer InternalHotbar { get; set; }
		public InventoryContainer Hotbar => InternalHotbar.Value;

		[Net] private NetInventoryContainer InternalEquipment { get; set; }
		public InventoryContainer Equipment => InternalEquipment.Value;

		[Net] public NetInventoryContainer ChestInventory { get; private set; }

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

		private TimeSince TimeSinceBackpackOpen { get; set; }
		private bool IsBackpackToggleMode { get; set; }
		private bool IsWaitingToRespawn { get; set; }
		private BlockGhost BlockGhost { get; set; }
		private UI.Nameplate Nameplate { get; set; }

		private FirstPersonCamera FirstPersonCamera { get; set; } = new();
		private SpectateCamera SpectateCamera { get; set; } = new();

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
			HotbarIndex = 0;
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
			
			if ( Hotbar.Give( item ) )
				return true;

			return Backpack.Give( item );
		}

		public void TryGiveAmmo( AmmoType type, ushort amount )
		{
			var item = InventorySystem.CreateItem<AmmoItem>();
			item.AmmoType = type;
			item.StackSize = amount;

			var remaining = Hotbar.Stack( item );

			if ( remaining > 0 )
			{
				Backpack.Stack( item );
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

			var remaining = Hotbar.Stack( item );

			if ( remaining > 0 )
			{
				Backpack.Stack( item );
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
			return Equipment.Give( item, (ushort)slotToIndex );
		}

		public ushort TryGiveItem( InventoryItem item )
		{
			var remaining = Hotbar.Stack( item );

			if ( remaining > 0 )
			{
				remaining = Backpack.Stack( item );
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
			items.AddRange( Hotbar.FindItems<T>() );
			items.AddRange( Backpack.FindItems<T>() );
			items.AddRange( Equipment.FindItems<T>() );
			return items;
		}

		public List<InventoryItem> FindItems( Type type )
		{
			var items = new List<InventoryItem>();
			items.AddRange( Hotbar.FindItems( type ) );
			items.AddRange( Backpack.FindItems( type ) );
			items.AddRange( Equipment.FindItems( type ) );
			return items;
		}

		public int TakeResources( Type type, int count )
		{
			var items = new List<ResourceItem>();

			items.AddRange( Hotbar.FindItems<ResourceItem>() );
			items.AddRange( Backpack.FindItems<ResourceItem>() );

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

			items.AddRange( Hotbar.FindItems<AmmoItem>() );
			items.AddRange( Backpack.FindItems<AmmoItem>() );

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

			items.AddRange( Hotbar.FindItems<AmmoItem>() );
			items.AddRange( Backpack.FindItems<AmmoItem>() );

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
		public void ShowHitMarker( bool isHeadshot )
		{
			if ( isHeadshot )
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
				var draw = Util.Draw.Reset();
				draw.BlendMode = BlendMode.Lighten;
				draw.Color = Color.Yellow.WithAlpha( 0.6f );
				draw.Circle( screenSize * 0.5f, 6f );
			}
		}

		public virtual void ClearInventories()
		{
			Equipment.RemoveAll();
			Backpack.RemoveAll();
			Hotbar.RemoveAll();
			ChestInventory.Value.RemoveAll();
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
		}

		public override void Spawn()
		{
			EnableTouchPersists = true;
			EnableDrawing = false;
			EnableTouch = true;

			SetModel( "models/citizen/citizen.vmdl" );
			AttachClothing( "models/citizen_clothes/shoes/slippers/models/slippers.vmdl" );
			SetMaterialGroup( Rand.Int( MaterialGroupCount - 1 ) );

			Tags.Add( "player" );

			base.Spawn();
		}

		public override void OnKilled()
		{
			if ( LastDamageTaken.Attacker is Player attacker )
			{
				var resources = FindItems<ResourceItem>();

				foreach ( var resource in resources )
				{
					resource.Parent.Remove( resource );
					attacker.TryGiveItem( resource );
				}

				UI.Hud.AddKillFeed( To.Everyone, attacker, this, LastDamageTaken.Weapon, LastDamageTaken.Flags );
			}
			else
			{
				UI.Hud.AddKillFeed( To.Everyone, this, LastDamageTaken.Flags );
			}

			BecomeRagdollOnClient( LastDamageTaken.Force, LastDamageTaken.BoneIndex );

			EnableAllCollisions = false;
			EnableDrawing = false;
			Controller = null;

			UI.RespawnScreen.Show( To.Single( this ), 5f, LastDamageTaken.Attacker, LastDamageTaken.Weapon );

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
			Nameplate = new UI.Nameplate( this );

			base.ClientSpawn();
		}

		public override void Respawn()
		{
			var isLobbyState = Game.IsState<LobbyState>();

			UI.RespawnScreen.Hide( To.Single( this ) );

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

			ClearBuffs();

			var spawnpoint = GetSpawnpoint();

			if ( spawnpoint.HasValue )
			{
				Transform = spawnpoint.Value;
			}

			InitializeHotbarWeapons();
			ResetInterpolation();
		}

		public override void FrameSimulate( Client client )
		{
			if ( LifeState == LifeState.Alive )
				FirstPersonCamera?.Update();
			else
				SpectateCamera?.Update();

			Controller?.SetActivePlayer( this );
			Controller?.FrameSimulate();
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

			Controller?.SetActivePlayer( this );
			Controller?.Simulate();

			SimulateAnimation();
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

				if ( info.Hitbox.HasTag( "head" ) )
				{
					info.Damage *= 2f;
				}

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

				attacker.ShowHitMarker( To.Single( attacker ), info.Hitbox.HasTag( "head" ) );

				UI.FloatingDamage.Show( this, info.Damage, info.Position );
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
			var container = Hotbar;
			var blockItem = container.GetFromSlot( HotbarIndex ) as BlockItem;

			if ( blockItem.IsValid() )
			{
				var position = GetBlockPosition( EyePosition, EyeRotation.Forward );
				var ghost = GetOrCreateBlockGhost();

				if ( position.HasValue )
				{
					var block = world.GetBlockType( position.Value );

					if ( block is AirBlock )
					{
						ghost.EnableDrawing = true;
						ghost.Position = world.ToSourcePosition( position.Value );
						ghost.Color = Team.GetColor();
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
			var position = GetBlockPosition( EyePosition, EyeRotation.Forward );
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
			Projectiles.Simulate();

			SimulateActiveChild( ActiveChild );

			if ( ActiveChildInput.IsValid() && ActiveChildInput.Owner == this )
				ActiveChild = ActiveChildInput;

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
					var container = Hotbar;
					var item = container.GetFromSlot( HotbarIndex );

					if ( item.IsValid() )
					{
						if ( item is BlockItem blockItem )
						{
							TryPlaceBlockItem( client, blockItem, EyePosition, EyeRotation.Forward );
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
					var container = Hotbar;
					var item = container.GetFromSlot( HotbarIndex );

					if ( item.IsValid() && item.CanBeDropped )
					{
						var itemToDrop = item;

						if ( item.StackSize > 1 )
						{
							itemToDrop = InventorySystem.DuplicateItem( item );
							itemToDrop.StackSize = 1;
							item.StackSize--;
						}

						var entity = new ItemEntity();
						entity.Position = EyePosition + EyeRotation.Forward * 32f;
						entity.SetItem( itemToDrop );
						entity.ApplyLocalImpulse( EyeRotation.Forward * 100f + Vector3.Up * 50f );
					}

					PlaySound( "item.dropped" );
				}
			}
			else if ( Prediction.FirstTime )
			{
				if ( Input.Pressed( InputButton.Score ) )
				{
					if ( !UI.Backpack.Current.IsOpen )
						TimeSinceBackpackOpen = 0f;
					else
						IsBackpackToggleMode = false;

					if ( UI.IDialog.IsActive() )
						UI.IDialog.CloseActive();
					else
						UI.Backpack.Current?.Open();
				}

				if ( Input.Released( InputButton.Score ) )
				{
					if ( TimeSinceBackpackOpen <= 0.2f )
					{
						IsBackpackToggleMode = true;
					}

					if ( !IsBackpackToggleMode )
					{
						UI.Backpack.Current?.Close();
					}
				}

				if ( Input.Down( InputButton.Menu ) )
					UI.TeamList.Open();
				else
					UI.TeamList.Close();
			}

			var currentSlotIndex = (int)HotbarIndex;

			if ( Input.MouseWheel > 0 )
				currentSlotIndex++;
			else if ( Input.MouseWheel < 0 )
				currentSlotIndex--;

			var maxSlotIndex = Hotbar.SlotLimit - 1;

			if ( currentSlotIndex < 0 )
				currentSlotIndex = maxSlotIndex;
			else if ( currentSlotIndex > maxSlotIndex )
				currentSlotIndex = 0;


			HotbarIndex = (ushort)currentSlotIndex;
			UpdateHotbarSlotKeys();

			var hotbarItem = Hotbar.GetFromSlot( HotbarIndex );

			if ( hotbarItem is WeaponItem weaponItem )
				ActiveChild = weaponItem.Weapon;
			else
				ActiveChild = null;

			if ( IsClient && Prediction.FirstTime )
			{
				if ( Input.Released( InputButton.Use ) )
				{
					if ( !UI.IDialog.IsActive() )
					{
						var trace = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 10000f )
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
						UI.IDialog.CloseActive();
					}
				}
			}
		}

		protected virtual void OnTeamChanged( Team team )
		{

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
			var hotbar = new HotbarContainer();
			hotbar.SetEntity( this );
			hotbar.SetSlotLimit( 8 );
			hotbar.AddConnection( Client );
			hotbar.ItemTaken += OnHotbarItemTaken;
			hotbar.ItemGiven += OnHotbarItemGiven;
			InventorySystem.Register( hotbar );

			InternalHotbar = new NetInventoryContainer( hotbar );

			var backpack = new BackpackContainer();
			backpack.SetEntity( this );
			backpack.SetSlotLimit( 24 );
			backpack.AddConnection( Client );
			backpack.ItemTaken += OnBackpackItemTaken;
			backpack.ItemGiven += OnBackpackItemGiven;
			InventorySystem.Register( backpack );

			InternalBackpack = new NetInventoryContainer( backpack );

			var chest = new InventoryContainer();
			chest.SetEntity( this );
			chest.SetSlotLimit( 24 );
			chest.AddConnection( Client );
			InventorySystem.Register( chest );

			ChestInventory = new NetInventoryContainer( chest );

			var equipment = new EquipmentContainer( );
			equipment.SetEntity( this );
			equipment.SetSlotLimit( 3 );
			equipment.AddConnection( Client );
			equipment.ItemTaken += OnEquipmentItemTaken;
			equipment.ItemGiven += OnEquipmentItemGiven;
			InventorySystem.Register( equipment );

			InternalEquipment = new NetInventoryContainer( equipment );
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

			volume *= GetFootstepVolume();

			TimeSinceLastFootstep = 0f;

			var trace = Trace.Ray( position, position + Vector3.Down * 20f )
				.Radius( 1 )
				.Ignore( this )
				.Run();

			if ( !trace.Hit ) return;

			trace.Surface.DoFootstep( this, trace, foot, volume );
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

			var remaining = TryGiveItem( itemEntity.Item );

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
			ViewAngles = Angles.Zero;
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
				InventorySystem.Remove( Hotbar, true );
				InventorySystem.Remove( Backpack, true );
				InventorySystem.Remove( Equipment, true );
				InventorySystem.Remove( ChestInventory.Value, true );
			}

			base.OnDestroy();
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
			if ( instance is ArmorItem armor && !Equipment.Is( instance.Parent ) )
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
				if ( weapon.Weapon.IsValid() && !Hotbar.Is( instance.Parent ) )
				{
					weapon.Weapon.Delete();
					weapon.Weapon = null;
					weapon.IsDirty = true;
				}
			}
		}

		private void InitializeHotbarWeapons()
		{
			foreach ( var item in Hotbar.ItemList )
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
			var index = HotbarIndex;

			if ( Input.Pressed( InputButton.Slot1 ) )
				index = (ushort)Math.Min( 0, Hotbar.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot2 ) )
				index = (ushort)Math.Min( 1, Hotbar.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot3 ) )
				index = (ushort)Math.Min( 2, Hotbar.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot4 ) )
				index = (ushort)Math.Min( 3, Hotbar.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot5 ) )
				index = (ushort)Math.Min( 4, Hotbar.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot6 ) )
				index = (ushort)Math.Min( 5, Hotbar.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot7 ) )
				index = (ushort)Math.Min( 6, Hotbar.SlotLimit - 1 );

			if ( Input.Pressed( InputButton.Slot8 ) )
				index = (ushort)Math.Min( 7, Hotbar.SlotLimit - 1 );

			if ( index != HotbarIndex )
			{
				var container = Hotbar;
				var item = container.GetFromSlot( index );

				if ( item is IConsumableItem consumable )
				{
					if ( IsServer )
					{
						consumable.Consume( this );
					}

					return;
				}

				HotbarIndex = index;
			}
		}
	}
}
