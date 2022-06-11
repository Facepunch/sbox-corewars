using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class SwordShopItemTier3 : BaseWeaponShopItem<SwordItemTier3>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 70
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );
	}
}
