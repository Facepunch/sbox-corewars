using Facepunch.CoreWars.Inventory;

namespace Facepunch.CoreWars
{
	public class ArmorChestShopItemTier1 : BaseArmorShopItem<ArmorChestTier1>
	{
		public override string Name => "Leather Chest Armor";
		public override string Description => "A low protection chest armor piece.";

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_chest_1.png";
		}
	}
}
