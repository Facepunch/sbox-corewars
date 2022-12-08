
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class CrystalItem : ResourceItem
	{
		public override ushort MaxStackSize => 64;
		public override string UniqueId => "item_crystal";
		public override string Description => "Rare crystals used for purchasing special items and blocks.";
		public override string Name => "Crystal";
		public override string Icon => "textures/items/crystal.png";

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}
	}
}
