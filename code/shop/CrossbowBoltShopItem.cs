using System;
using System.Collections.Generic;
using Facepunch.CoreWars.Inventory;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrossbowBoltShopItem : BaseShopItem
	{
		public override string Name => AmmoType.Bolt.ToString();
		public override string Description => AmmoType.Bolt.GetDescription();
		public override ItemTag[] Tags => new ItemTag[] { ItemTag.CanDrop };
		public override int Quantity => 8;
		public override Color Color => ColorPalette.Ammo;
		public override int SortOrder => 4;
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
			item.StackSize = (ushort)Quantity;
			player.TryGiveItem( item );
		}
	}
}
