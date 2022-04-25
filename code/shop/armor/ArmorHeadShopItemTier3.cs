using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public class ArmorHeadShopItemTier3 : BaseArmorShopItem<ArmorHeadTier3>
	{
		public override string Name => "Metal Head Armor";
		public override string Description => "A heavy protection head armor piece.";
		public override Type PreviousArmorType => typeof( ArmorHeadTier2 );

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_head_3.png";
		}
	}
}
