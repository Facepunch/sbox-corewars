using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public class ArmorLegsShopItemTier2 : BaseArmorShopItem<ArmorLegsTier2>
	{
		public override string Name => "Kevlar Legs Armor";
		public override string Description => "A medium protection legs armor piece.";
		public override Type PreviousArmorType => typeof( ArmorLegsTier1 );

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_legs_2.png";
		}
	}
}
