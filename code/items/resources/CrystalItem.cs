using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crystal" )]
	public class CrystalItem : ResourceItem
	{
		public override ushort MaxStackSize => 32;

		public override string GetName()
		{
			return "Crystal";
		}

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}

		public override string GetIcon()
		{
			return "textures/items/crystal.png";
		}
	}
}
