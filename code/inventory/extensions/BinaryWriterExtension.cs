using Sandbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.CoreWars.Inventory
{
	public static class BinaryWriterExtension
	{
		public static void WriteInventoryItem( this BinaryWriter writer, InventoryItem item )
		{
			if ( item != null )
			{
				writer.Write( item.LibraryId );
				writer.Write( item.StackSize );
				writer.Write( item.ItemId );
				writer.Write( item.SlotId );

				item.Write( writer );
			}
			else
			{
				writer.Write( -1 );
			}
		}

		public static void WriteInventoryContainer( this BinaryWriter writer, InventoryContainer container )
		{
			writer.Write( container.InventoryId );
			writer.Write( container.SlotLimit );
			writer.Write( container.Entity.NetworkIdent );

			for ( var i = 0; i < container.SlotLimit; i++ )
			{
				var instance = container.ItemList[i];

				if ( instance != null )
				{
					writer.Write( true );
					writer.WriteInventoryItem( instance );
				}
				else
				{
					writer.Write( false );
				}
			}
		}

	}
}
