using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorChestShopItemTier3 : BaseArmorShopItem<ArmorChestTier3>
	{
		public override string Name => "Heavy Chest Armor";
		public override string Description => "A heavy protection chest armor piece.";
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
