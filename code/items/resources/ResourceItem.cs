using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ResourceItem : InventoryItem
	{
		public override bool DropOnDeath => true;
	}
}
