using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_iron" )]
	public class IronItem : ResourceItem
	{
		public override ushort MaxStackSize => 32;

		public override string GetName()
		{
			return "Iron";
		}

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}

		public override string GetIcon()
		{
			return "textures/items/iron.png";
		}
	}
}
