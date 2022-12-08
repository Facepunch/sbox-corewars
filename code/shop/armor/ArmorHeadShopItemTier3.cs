
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorHeadShopItemTier3 : BaseArmorShopItem<ArmorHeadTier3>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 3
		};
	}
}
