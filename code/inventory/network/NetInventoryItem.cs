using Sandbox;

namespace Facepunch.CoreWars.Inventory
{
	public class NetInventoryItem : BaseNetworkable, INetworkSerializer, IValid
	{
		public InventoryItem Instance { get; private set; }
		public bool Initialized { get; private set; }

		public bool IsValid => Instance.IsValid();

		public NetInventoryItem()
		{

		}

		public NetInventoryItem( InventoryItem item )
		{
			Instance = item;
		}

		public void Read( ref NetRead read )
		{
			if ( Initialized ) return;
			var totalBytes = read.Read<int>();
			var output = new byte[totalBytes];
			Instance = InventoryItem.Deserialize( read.ReadUnmanagedArray( output ) );
			Initialized = true;
		}

		public void Write( NetWrite write )
		{
			var serialized = Instance.Serialize();
			write.Write( serialized.Length );
			write.Write( serialized );
		}
	}
}
