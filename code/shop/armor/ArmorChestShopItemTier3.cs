
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorChestShopItemTier3 : BaseArmorShopItem<ArmorChestTier3>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 3
		};
	}
}
