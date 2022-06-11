using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorChestShopItemTier2 : BaseArmorShopItem<ArmorChestTier2>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
	}
}
