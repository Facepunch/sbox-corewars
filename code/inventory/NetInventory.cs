using Sandbox;

namespace Facepunch.CoreWars.Inventory
{
	public class NetInventory : BaseNetworkable, INetworkSerializer, IValid
	{
		public InventoryContainer Container { get; private set; }
		public bool Initialized { get; private set; }

		public bool IsValid => Container.IsValid();

		public NetInventory()
		{

		}

		public NetInventory( InventoryContainer container )
		{
			Container = container;
		}

		public void Read( ref NetRead read )
		{
			if ( Initialized ) return;
			var totalBytes = read.Read<int>();
			var output = new byte[totalBytes];
			Container = InventoryContainer.Deserialize( read.ReadUnmanagedArray( output ) );
			Initialized = true;
		}

		public void Write( NetWrite write )
		{
			var serialized = Container.Serialize();
			write.Write( serialized.Length );
			write.Write( serialized );
		}
	}
}
