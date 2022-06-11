using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrossbowShopItemTier1 : BaseWeaponShopItem<CrossbowItemTier1>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
		public override AmmoType AmmoType => AmmoType.Bolt;
		public override int AmmoAmount => 8;
	}
}
