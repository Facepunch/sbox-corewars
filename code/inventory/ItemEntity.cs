using Sandbox;
using System.IO;
using Facepunch.CoreWars.UI;

namespace Facepunch.CoreWars;

public partial class ItemEntity : ModelEntity, IResettable
{
	[Net] private NetInventoryItem InternalItem { get; set; }
	public InventoryItem Item => InternalItem.Value;

	public TimeUntil TimeUntilCanPickup { get; set; }
	public Vector3 IconPosition { get; set; }

	private PickupTrigger PickupTrigger { get; set; }
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
		var worldModel = !string.IsNullOrEmpty( item.WorldModel ) ? item.WorldModel : "models/sbox_props/burger_box/burger_box.vmdl";

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

	public override void Spawn()
	{
		TimeUntilCanPickup = 1f;
		Transmit = TransmitType.Always;

		Tags.Add( "item", "solid", "passplayers" );

		base.Spawn();
	}
}

