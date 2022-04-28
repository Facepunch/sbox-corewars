using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_armor_chest_3" )]
	public class ArmorChestTier3 : ArmorItem
	{
		public override float DamageMultiplier => 0.3f;
		public override ArmorSlot ArmorSlot => ArmorSlot.Chest;
		public override string Name => "Heavy Chest Armor";
		public override string Icon => "textures/items/armor_chest_3.png";
		public override int ArmorTier => 3;
	}
}
