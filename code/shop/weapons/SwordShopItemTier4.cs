using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class SwordShopItemTier4 : BaseWeaponShopItem<SwordItemTier4>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 60
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );
	}
}
