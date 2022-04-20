using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_gold" )]
	public class GoldItem : ResourceItem
	{
		public override ushort MaxStackSize => 32;
		public override string Name => "Gold";
		public override string Icon => "textures/items/gold.png";

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}
	}
}
