using Facepunch.CoreWars.Inventory;
using System;

namespace Facepunch.CoreWars
{
	public class ArmorHeadShopItemTier2 : BaseArmorShopItem<ArmorHeadTier2>
	{
		public override string Name => "Medium Head Armor";
		public override string Description => "A medium protection head armor piece.";
		public override Type PreviousArmorType => typeof( ArmorHeadTier1 );
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_head_2.png";
		}
	}
}
