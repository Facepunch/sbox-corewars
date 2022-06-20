using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_armor_legs_3" )]
	public class ArmorLegsTier3 : ArmorItem
	{
		public override float DamageMultiplier => 0.3f;
		public override ArmorSlot ArmorSlot => ArmorSlot.Legs;
		public override string Name => "Heavy Legs Armor";
		public override string Description => "A heavy protection legs armor piece.";
		public override string Icon => "textures/items/armor_legs_3.png";
		public override string PrimaryModel => "models/citizen_clothes/trousers/legarmour/models/leg_armour.vmdl";
		public override int Tier => 3;
	}
}
