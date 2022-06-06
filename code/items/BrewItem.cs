using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public abstract class BrewItem : InventoryItem
	{
		public override bool CanBeDropped => false;
		public override ushort MaxStackSize => 4;

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}

		public virtual void OnConsumed( Player player )
		{
			StackSize--;

			if ( StackSize <= 0 )
				Remove();
		}
	}
}
