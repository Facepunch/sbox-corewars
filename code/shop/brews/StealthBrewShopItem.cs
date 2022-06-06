using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class StealthBrewShopItem : BaseShopItem
	{
		public override string Name => "Stealth Brew";
		public override string Description => "Provides invisibility for 30 seconds when consumed. Attacking or being damaged will nullify the effect.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 1
		};
		public override Type RequiredUpgradeType => typeof( BreweryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/brew_stealth.png";
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<StealthBrewItem>();
			player.TryGiveItem( item );
		}
	}
}
