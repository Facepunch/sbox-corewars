
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrossbowShopItemTier2 : BaseWeaponShopItem<CrossbowItemTier2>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 6
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );
	}
}
