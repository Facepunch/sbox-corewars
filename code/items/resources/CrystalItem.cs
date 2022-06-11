using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crystal" )]
	public class CrystalItem : ResourceItem
	{
		public override ushort MaxStackSize => 64;
		public override string Description => "Rare crystals used for purchasing special items and blocks.";
		public override string Name => "Crystal";
		public override string Icon => "textures/items/crystal.png";

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}
	}
}
