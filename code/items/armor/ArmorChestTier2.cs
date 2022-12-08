
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ArmorChestTier2 : ArmorItem
	{
		public override float DamageMultiplier => 0.5f;
		public override string UniqueId => "item_armor_chest_2";
		public override ArmorSlot ArmorSlot => ArmorSlot.Chest;
		public override string Name => "Medium Chest Armor";
		public override string Description => "A medium protection chest armor piece.";
		public override string Icon => "textures/items/armor_chest_2.png";
		public override string PrimaryModel => "models/citizen_clothes/vest/tactical_vest/models/tactical_vest.vmdl";
		public override int Tier => 2;
	}
}
