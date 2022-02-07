using System.IO;

namespace Facepunch.CoreWars.Inventory
{
	public class InventoryContainer
	{
		public uint ContainerId { get; private set; }

		public InventoryContainer( uint id )
		{
			ContainerId = id;
		}

		public virtual void Serialize( BinaryWriter writer )
		{

		}

		public virtual void Deserialize( BinaryReader reader )
		{

		}
	}
}
