using Sandbox;
using System;
using System.IO;
using System.Linq;
using Facepunch.CoreWars.Utility;

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
				PhysicsBody.LinearDamping = 4f;
				PhysicsBody.LinearDamping = 4f;
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
			Effect = Particles.Create( "particles/gameplay/items/item_on_ground/generic/items_on_ground.vpcf" );
			Effect.SetPosition( 0, WorldSpaceBounds.Center );

			Icon = new ItemWorldIcon( this );

			if ( Item.Instance.IsValid() )
			{
				Effect.SetPosition( 6, Item.Instance.Color.Saturate( 1.5f ) * 255f );
			}

			base.ClientSpawn();
		}

		[Event.Tick.Client]
		protected virtual void ClientTick()
		{
			IconPosition = WorldSpaceBounds.Center + Vector3.Up * (8f + MathF.Sin( Time.Now ) * 8f);
			Effect?.SetPosition( 0, IconPosition );
			Effect?.SetForward( 0, Vector3.Forward );
		}

		protected override void OnDestroy()
		{
			Effect?.Destroy( true );

			Icon?.Delete();
			Icon = null;

			base.OnDestroy();
		}
	}
}

