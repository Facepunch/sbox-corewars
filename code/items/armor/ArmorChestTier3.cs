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
		public override string Description => "A heavy protection chest armor piece.";
		public override string Icon => "textures/items/armor_chest_3.png";
		public override string PrimaryModel => "models/citizen_clothes/vest/chest_armour/models/chest_armour.vmdl";
		public override int Tier => 3;
	}
}
