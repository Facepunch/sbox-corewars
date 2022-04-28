using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_armor_chest_2" )]
	public class ArmorChestTier2 : ArmorItem
	{
		public override float DamageMultiplier => 0.5f;
		public override ArmorSlot ArmorSlot => ArmorSlot.Chest;
		public override string Name => "Medium Chest Armor";
		public override string Icon => "textures/items/armor_chest_2.png";
		public override int ArmorTier => 2;
	}
}
