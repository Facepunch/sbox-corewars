using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorChestShopItemTier1 : BaseArmorShopItem<ArmorChestTier1>
	{
		public override string Name => "Light Chest Armor";
		public override string Description => "A low protection chest armor piece.";
		public override Type NextArmorType => typeof( ArmorChestTier2 );
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_chest_1.png";
		}
	}
}
