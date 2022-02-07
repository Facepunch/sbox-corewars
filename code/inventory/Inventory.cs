using System.Collections.Generic;

namespace Facepunch.CoreWars.Inventory
{
	public static class Inventory
	{
		private static Dictionary<uint, InventoryContainer> Containers { get; set; } = new();
		private static Dictionary<uint, InventoryItem> Items { get; set; } = new();

		private static uint NextContainerId { get; set; }
		private static uint NextItemId { get; set; }

		public static InventoryContainer CreateContainer()
		{
			return CreateContainer( NextContainerId++ );
		}

		public static InventoryContainer CreateContainer( uint id )
		{
			if ( Containers.TryGetValue( id, out var container ) )
			{
				return container;
			}

			container = new InventoryContainer( id );
			Containers.Add( id, container );
			return container;
		}
	}
}
