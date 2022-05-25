using Sandbox;

namespace Facepunch.CoreWars.Inventory
{
	public class NetInventoryContainer : BaseNetworkable, INetworkSerializer, IValid
	{
		public InventoryContainer Instance { get; private set; }
		public bool Initialized { get; private set; }

		public bool IsValid => Instance.IsValid();

		public NetInventoryContainer()
		{

		}

		public NetInventoryContainer( InventoryContainer container )
		{
			Instance = container;
		}

		public bool Is( InventoryContainer container )
		{
			return container == Instance;
		}

		public bool Is( NetInventoryContainer container )
		{
			return container == this;
		}

		public void Read( ref NetRead read )
		{
			if ( Initialized ) return;
			var totalBytes = read.Read<int>();
			var output = new byte[totalBytes];
			Instance = InventoryContainer.Deserialize( read.ReadUnmanagedArray( output ) );
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
