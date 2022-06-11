using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_gold" )]
	public class GoldItem : ResourceItem
	{
		public override ushort MaxStackSize => 64;
		public override string Description => "Solid gold bars used for purchasing team upgrades.";
		public override string Name => "Gold";
		public override string Icon => "textures/items/gold.png";

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}
	}
}
