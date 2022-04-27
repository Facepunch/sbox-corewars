using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorLegsShopItemTier1 : BaseArmorShopItem<ArmorLegsTier1>
	{
		public override string Name => "Light Legs Armor";
		public override string Description => "A low protection legs armor piece.";
		public override Type NextArmorType => typeof( ArmorLegsTier2 );
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_legs_1.png";
		}
	}
}
