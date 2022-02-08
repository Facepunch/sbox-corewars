using Sandbox;

namespace Facepunch.CoreWars.Inventory
{
	public class NetInventory : BaseNetworkable, INetworkSerializer
	{
		public InventoryContainer Container { get; private set; }

		public NetInventory()
		{

		}

		public NetInventory( InventoryContainer container )
		{
			Container = container;
		}

		public void Read( ref NetRead read )
		{
			var totalBytes = read.Read<int>();
			var output = new byte[totalBytes];
			Container = InventoryContainer.Deserialize( read.ReadUnmanagedArray( output ) );
		}

		public void Write( NetWrite write )
		{
			var serialized = Container.Serialize();
			write.Write( serialized.Length );
			write.Write( serialized );
		}
	}
}
