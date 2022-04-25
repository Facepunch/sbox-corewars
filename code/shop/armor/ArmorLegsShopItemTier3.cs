using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public class ArmorLegsShopItemTier3 : BaseArmorShopItem<ArmorLegsTier3>
	{
		public override string Name => "Heavy Legs Armor";
		public override string Description => "A heavy protection legs armor piece.";
		public override Type PreviousArmorType => typeof( ArmorLegsTier2 );
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 3
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_legs_3.png";
		}
	}
}
