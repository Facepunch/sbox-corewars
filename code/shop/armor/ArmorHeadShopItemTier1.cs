using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorHeadShopItemTier1 : BaseArmorShopItem<ArmorHeadTier1>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};
	}
}
