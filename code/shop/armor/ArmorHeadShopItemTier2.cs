using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public class ArmorHeadShopItemTier2 : BaseArmorShopItem<ArmorHeadTier2>
	{
		public override string Name => "Security Head Armor";
		public override string Description => "A medium protection head armor piece.";
		public override Type PreviousArmorType => typeof( ArmorHeadTier1 );

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_head_2.png";
		}
	}
}
