﻿using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_gold" )]
	public class GoldItem : ResourceItem
	{
		public override string GetName()
		{
			return "Gold";
		}

		public override bool CanStackWith( InventoryItem other )
		{
			return (other is GoldItem);
		}

		public override string GetIcon()
		{
			return "textures/items/gold.png";
		}
	}
}
