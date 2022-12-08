
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class HealthBrewShopItem : BaseBrewShopItem<HealthBrewItem>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 8
		};
		public override Type RequiredUpgradeType => typeof( BreweryUpgrade );
	}
}
