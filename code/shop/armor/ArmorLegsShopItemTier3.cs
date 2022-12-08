
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorLegsShopItemTier3 : BaseArmorShopItem<ArmorLegsTier3>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 3
		};
	}
}
