
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class SwordShopItemTier2 : BaseWeaponShopItem<SwordItemTier2>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};
	}
}
