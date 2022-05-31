using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_armor_legs_2" )]
	public class ArmorLegsTier2 : ArmorItem
	{
		public override float DamageMultiplier => 0.5f;
		public override ArmorSlot ArmorSlot => ArmorSlot.Legs;
		public override string Name => "Medium Legs Armor";
		public override string Icon => "textures/items/armor_legs_2.png";
		public override string ModelName => "models/citizen_clothes/trousers/trousers.smart.vmdl";
		public override int Tier => 2;
	}
}
