using Sandbox;
using System;
using System.IO;

namespace Facepunch.CoreWars.Inventory
{
	public class ItemEntity : ModelEntity, INetworkSerializer
	{
		public InventoryItem Item { get; private set; }

		public void Read( ref NetRead read )
		{
			var data = new byte[read.Remaining];

			using ( var stream = new MemoryStream( read.ReadUnmanagedArray( data ) ) )
			{
				using ( var reader = new BinaryReader( stream ) )
				{
					Item = reader.ReadInventoryItem();
				}
			}

			Log.Info( "Received: " + Item.GetName() );
		}

		public void SetItem( InventoryItem item )
		{
			if ( string.IsNullOrEmpty( item.WorldModel ) )
			{
				throw new Exception( "Unable to create an item entity without a world model!" );
			}

			SetModel( item.WorldModel );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			Item = item;
			Item.WorldEntity = this;

			WriteNetworkData();
		}

		public InventoryItem Take()
		{
			if ( Item.IsValid() && Item.WorldEntity == this )
			{
				Item.WorldEntity = null;
				Delete();
			}

			return Item;
		}

		public override void Spawn()
		{
			base.Spawn();
		}

		public void Write( NetWrite write )
		{
			using ( var stream = new MemoryStream() )
			{
				using ( var writer = new BinaryWriter( stream ) )
				{
					writer.WriteInventoryItem( Item );
				}

				write.WriteUnmanagedArray( stream.ToArray() );
			}
		}
	}
}

