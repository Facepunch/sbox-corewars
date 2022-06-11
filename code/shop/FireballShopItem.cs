using System;
using System.Collections.Generic;
using Facepunch.CoreWars.Inventory;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class FireballShopItem : BaseWeaponShopItem<FireballItem>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 1,
			[typeof( IronItem )] = 4
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );
	}
}
