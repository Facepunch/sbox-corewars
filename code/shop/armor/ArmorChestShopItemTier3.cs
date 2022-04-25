using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public class ArmorChestShopItemTier3 : BaseArmorShopItem<ArmorChestTier3>
	{
		public override string Name => "Heavy Chest Armor";
		public override string Description => "A heavy protection chest armor piece.";
		public override Type PreviousArmorType => typeof( ArmorChestTier2 );
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 3
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_chest_3.png";
		}
	}
}
