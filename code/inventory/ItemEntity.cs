using Sandbox;
using System.IO;
using Facepunch.CoreWars.UI;
using System;

namespace Facepunch.CoreWars;

public partial class ItemEntity : ModelEntity, IResettable
{
	[Net] private NetInventoryItem InternalItem { get; set; }
	public InventoryItem Item => InternalItem.Value;

	public TimeUntil TimeUntilCanPickup { get; set; }
	public Vector3 IconPosition { get; set; }

	private ItemWorldIcon Icon { get; set; }
	private Particles Effect { get; set; }

	public void Serialize( BinaryWriter writer )
	{
		writer.Write( Transform );

		if ( Item.IsValid() )
		{
			writer.Write( true );
			writer.Write( Item );
		}
		else
		{
			writer.Write( false );
		}
	}

	public void Deserialize( BinaryReader reader )
	{
		Transform = reader.ReadTransform();

		var isValid = reader.ReadBoolean();

		
		if ( isValid )
		{
			var item = reader.ReadInventoryItem();
			SetItem( item );
		}
		else
		{
			Delete();
		}
	}

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

		InternalItem = new NetInventoryItem( item );
		item.SetWorldEntity( this );
	}

	public InventoryItem Take()
	{
		if ( IsValid && Item.IsValid() )
		{
			var item = Item;

			item.ClearWorldEntity();
			InternalItem = null;
			Delete();

			return item;
		}

		return null;
	}

	public virtual void Reset()
	{
		Delete();
	}

	public override void ClientSpawn()
	{
		Effect = Particles.Create( "particles/gameplay/items/item_on_ground/generic/items_on_ground.vpcf" );
		Icon = new ItemWorldIcon( this );

		if ( Item.IsValid() )
		{
			Effect.SetPosition( 6, Item.Color.Saturate( 1.5f ) * 255f );
		}
	}

	public override void Spawn()
	{
		TimeUntilCanPickup = 1f;
		Transmit = TransmitType.Always;

		Tags.Add( "item", "solid", "passplayers" );

		base.Spawn();
	}

	protected override void OnDestroy()
	{
		Effect?.Destroy( true );
		Icon?.Delete();
		Icon = null;
	}

	[Event.Tick.Client]
	protected virtual void ClientTick()
	{
		IconPosition = WorldSpaceBounds.Center + Vector3.Up * (8f + MathF.Sin( Time.Now ) * 8f);
		Effect?.SetPosition( 0, IconPosition );
		Effect?.SetForward( 0, Vector3.Forward );
	}
}
