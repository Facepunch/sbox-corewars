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
				SetupPhysicsFromSphere( PhysicsMotionType.Dynamic, Vector3.Zero, 4f );
			}

			Item = new NetInventoryItem( item );
			item.SetWorldEntity( this );

			CollisionGroup = CollisionGroup.Weapon;
			SetInteractsAs( CollisionLayer.Debris );
			EnableTouch = true;
		}

		public InventoryItem Take()
		{
			if ( !IsValid )
				return null;

			Delete();

			var item = Item.Instance;

			if ( item.IsValid() && item.WorldEntity == this )
			{
				item.ClearWorldEntity();
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
				if ( TimeUntilCanPickup )
				{
					var item = Take();

					if ( item.IsValid() )
					{
						player.TryGiveItem( item );
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

