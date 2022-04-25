using Facepunch.CoreWars.Inventory;

namespace Facepunch.CoreWars
{
	public class ArmorHeadShopItemTier1 : BaseArmorShopItem<ArmorHeadTier1>
	{
		public override string Name => "Hardhat Head Armor";
		public override string Description => "A low protection head armor piece.";

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_head_1.png";
		}
	}
}
