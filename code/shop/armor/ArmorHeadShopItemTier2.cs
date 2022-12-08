
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class ArmorHeadShopItemTier2 : BaseArmorShopItem<ArmorHeadTier2>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
	}
}
