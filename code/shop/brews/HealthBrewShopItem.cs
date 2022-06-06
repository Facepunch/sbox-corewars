using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class HealthBrewShopItem : BaseShopItem
	{
		public override string Name => "Health Brew";
		public override string Description => "Restores 30% of maximum health when consumed.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 8
		};
		public override Type RequiredUpgradeType => typeof( BreweryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/brew_health.png";
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<HealthBrewItem>();
			player.TryGiveItem( item );
		}
	}
}
