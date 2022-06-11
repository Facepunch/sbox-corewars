using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class DamageBrewShopItem : BaseBrewShopItem<DamageBrewItem>
	{
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 32
		};
		public override Type RequiredUpgradeType => typeof( BreweryUpgrade );
	}
}
