using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class SpeedBrewShopItem : BaseBrewShopItem<SpeedBrewItem>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};
		public override Type RequiredUpgradeType => typeof( BreweryUpgrade );
	}
}
