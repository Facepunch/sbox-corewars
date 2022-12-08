
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ArmorChestTier1 : ArmorItem
	{
		public override float DamageMultiplier => 0.7f;
		public override string UniqueId => "item_armor_chest_1";
		public override ArmorSlot ArmorSlot => ArmorSlot.Chest;
		public override string SecondaryModel => "models/citizen_clothes/shirt/chainmail/models/chainmail.vmdl";
		public override string PrimaryModel => "models/citizen_clothes/vest/cardboard_chest/models/cardboard_chest.vmdl";
		public override string Description => "A low protection chest armor piece.";
		public override string Name => "Light Chest Armor";
		public override string Icon => "textures/items/armor_chest_1.png";
		public override int Tier => 1;
	}
}
