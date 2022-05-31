using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_armor_chest_1" )]
	public class ArmorChestTier1 : ArmorItem
	{
		public override float DamageMultiplier => 0.7f;
		public override ArmorSlot ArmorSlot => ArmorSlot.Chest;
		public override string ModelName => "models/citizen_clothes/shirt/chainmail/models/chainmail.vmdl";
		public override string Name => "Light Chest Armor";
		public override string Icon => "textures/items/armor_chest_1.png";
		public override int Tier => 1;
	}
}
