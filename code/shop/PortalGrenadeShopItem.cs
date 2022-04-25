﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Facepunch.Voxels;
using Facepunch.CoreWars.Inventory;

namespace Facepunch.CoreWars
{
	public abstract class PortalGrenadeShopItem : BasePurchasable
	{
		public override string GetIcon( Player player )
		{
			return string.Empty;
		}

		public override void OnPurchased( Player player )
		{
			var item = InventorySystem.CreateItem<BlockItem>();
			var block = GetBlockType( player );

			item.StackSize = (ushort)Quantity;
			item.BlockId = block.BlockId;

			player.TryGiveItem( item );
		}
	}
}