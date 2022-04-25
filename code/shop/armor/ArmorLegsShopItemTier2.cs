using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public class ArmorLegsShopItemTier2 : BaseArmorShopItem<ArmorLegsTier2>
	{
		public override string Name => "Medium Legs Armor";
		public override string Description => "A medium protection legs armor piece.";
		public override Type PreviousArmorType => typeof( ArmorLegsTier1 );
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_legs_2.png";
		}
	}
}
