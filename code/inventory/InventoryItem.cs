using System.IO;

namespace Facepunch.CoreWars.Inventory
{
	public class InventoryItem
	{
		public InventoryContainer Container { get; private set; }
		public uint ItemId { get; private set; }
		public uint Slot { get; private set; }

		public virtual void Serialize( BinaryWriter writer )
		{

		}

		public virtual void Deserialize( BinaryReader reader )
		{

		}
	}
}
