using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_iron" )]
	public class IronItem : InventoryItem
	{
		public override string GetName()
		{
			return "Iron";
		}

		public override bool CanStackWith( InventoryItem other )
		{
			return (other is IronItem);
		}

		public override string GetIcon()
		{
			return string.Empty;
		}
	}
}
