using Sandbox;
using System;
using System.IO;
using System.Linq;

namespace Facepunch.CoreWars.Inventory
{
	public partial class ItemEntity : ModelEntity, IResettable
	{
		[Net] public NetInventoryItem Item { get; private set; }

		public TimeUntil TimeUntilCanPickup { get; set; }
		public Vector3 IconPosition { get; set; }

		private PickupTrigger PickupTrigger { get; set; }
		private ItemWorldIcon Icon { get; set; }
		private Particles Effect { get; set; }

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

				PhysicsBody.LinearDrag = 1f;
				PhysicsBody.LinearDamping = 0.9f;
				PhysicsBody.AngularDrag = 1f;
				PhysicsBody.LinearDamping = 0.9f;
			}

			PickupTrigger = new PickupTrigger
			{
				Parent = this,
				Position = Position
			};

			Item = new NetInventoryItem( item );
			item.SetWorldEntity( this );

			CollisionGroup = CollisionGroup.Weapon;
			SetInteractsAs( CollisionLayer.Debris );
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

		public virtual void Reset()
		{
			Delete();
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

			Effect = Particles.Create( "particles/gameplay/items/item_on_ground/item_on_ground.vpcf", this );
			Effect.SetPosition( 0, WorldSpaceBounds.Center );

			base.ClientSpawn();
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			IconPosition = WorldSpaceBounds.Center + Vector3.Up * (12f + MathF.Sin( Time.Now ) * 8f);
			Effect?.SetPosition( 0, IconPosition );
		}

		protected override void OnDestroy()
		{
			Icon?.Delete();
			Icon = null;

			base.OnDestroy();
		}
	}
}

