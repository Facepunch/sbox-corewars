using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class DamageBrewShopItem : BaseShopItem
	{
		public override string Name => "Damage Brew";
		public override string Description => "Gives a boost to damage output for 30 seconds when consumed.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 32
		};
		public override Type RequiredUpgradeType => typeof( BreweryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/brew_damage.png";
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<DamageBrewItem>();
			player.TryGiveItem( item );
		}
	}
}
