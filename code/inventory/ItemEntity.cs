using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars.Inventory
{
	public partial class ItemEntity : ModelEntity
	{
		[Net] public NetInventoryItem Item { get; private set; }

		public TimeUntil TimeUntilCanPickup { get; set; }

		private ItemWorldIcon Icon { get; set; }

		public void SetItem( InventoryItem item )
		{
			if ( !string.IsNullOrEmpty( item.WorldModel ) )
			{
				SetModel( item.WorldModel );
				SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			}
			else
			{
				SetupPhysicsFromSphere( PhysicsMotionType.Dynamic, Vector3.Zero, 8f );
			}

			Item = new NetInventoryItem( item );
			item.SetWorldEntity( this );

			CollisionGroup = CollisionGroup.Weapon;
			SetInteractsAs( CollisionLayer.Debris );
			EnableTouch = true;
		}

		public InventoryItem Take()
		{
			if ( IsValid )
			{
				var item = Item.Instance;

				item.ClearWorldEntity();
				Item = null;
				Delete();

				return item;
			}

			return null;
		}

		public override void Spawn()
		{
			TimeUntilCanPickup = 1f;

			base.Spawn();
		}

		public override void ClientSpawn()
		{
			if ( Item.Instance.IsValid() )
			{
				Icon = new ItemWorldIcon( this );
			}

			base.ClientSpawn();
		}

		public override void StartTouch( Entity other )
		{
			if ( IsServer && other is Player player )
			{
				if ( TimeUntilCanPickup && Item.IsValid() )
				{
					var remaining = player.TryGiveItem( Item.Instance );

					if ( remaining == 0 )
					{
						Item.Instance.ClearWorldEntity();
						Item = null;
						Delete();
					}
				}
			}

			base.StartTouch( other );
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			
		}

		protected override void OnDestroy()
		{
			Icon?.Delete();
			Icon = null;

			base.OnDestroy();
		}
	}
}

