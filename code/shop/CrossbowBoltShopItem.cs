using System;
using System.Collections.Generic;
using Facepunch.CoreWars.Inventory;

namespace Facepunch.CoreWars
{
	public class CrossbowBoltShopItem : BasePurchasable
	{
		public override string Name => "Crossbow Bolt";
		public override string Description => "Ammo for Crossbow weapons.";
		public override int Quantity => 8;
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/ammo_bolt.png";
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<AmmoItem>();
			item.AmmoType = AmmoType.Bolt;
			item.StackSize = Quantity;
			player.TryGiveItem( item );
		}
	}
}
