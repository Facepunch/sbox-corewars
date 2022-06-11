using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class PickaxeShopItemTier3 : BaseWeaponShopItem<PickaxeItemTier3>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 60
		};
	}
}
