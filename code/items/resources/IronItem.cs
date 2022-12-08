using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class IronItem : ResourceItem
	{
		public override ushort MaxStackSize => 64;
		public override string UniqueId => "item_iron";
		public override string Description => "Solid iron ingots used for purchasing basic items and blocks.";
		public override string Name => "Iron";
		public override string Icon => "textures/items/iron.png";

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}
	}
}
