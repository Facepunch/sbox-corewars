using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class StealthBrewShopItem : BaseBrewShopItem<StealthBrewItem>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 1
		};
		public override Type RequiredUpgradeType => typeof( BreweryUpgrade );
	}
}
