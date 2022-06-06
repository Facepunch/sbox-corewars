using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class SpeedBrewShopItem : BaseShopItem
	{
		public override string Name => "Speed Brew";
		public override string Description => "Gives a boost to movement speed for 30 seconds when consumed.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};
		public override Type RequiredUpgradeType => typeof( BreweryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/brew_speed.png";
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<SpeedBrewItem>();
			player.TryGiveItem( item );
		}
	}
}
