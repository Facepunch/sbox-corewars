using System;
using System.Collections.Generic;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class PortalShopItem : BaseWeaponShopItem<PortalItem>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 1
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );
	}
}
