using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public class ArmorChestShopItemTier2 : BaseArmorShopItem<ArmorChestTier2>
	{
		public override string Name => "Medium Chest Armor";
		public override string Description => "A medium protection chest armor piece.";
		public override Type PreviousArmorType => typeof( ArmorChestTier1 );
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_chest_2.png";
		}
	}
}
