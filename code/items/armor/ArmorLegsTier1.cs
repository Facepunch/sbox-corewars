
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	public class ArmorLegsTier1 : ArmorItem
	{
		public override float DamageMultiplier => 0.7f;
		public override string UniqueId => "item_armor_legs_1";
		public override ArmorSlot ArmorSlot => ArmorSlot.Legs;
		public override string Name => "Light Legs Armor";
		public override string Description => "A low protection legs armor piece.";
		public override string Icon => "textures/items/armor_legs_1.png";
		//public override string SecondaryModel => "models/citizen_clothes/trousers/trousers.jeans.vmdl";
		public override string PrimaryModel => "models/citizen_clothes/trousers/cardboardtrousers/models/cardboard_trousers.vmdl";
		public override int Tier => 1;
	}
}
