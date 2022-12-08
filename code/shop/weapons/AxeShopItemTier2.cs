
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class AxeShopItemTier2 : BaseWeaponShopItem<AxeItemTier2>
	{
		public override int SortOrder => 5;

		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 30
		};
	}
}
