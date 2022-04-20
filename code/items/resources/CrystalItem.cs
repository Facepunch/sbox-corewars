﻿using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crystal" )]
	public class CrystalItem : ResourceItem
	{
		public override ushort MaxStackSize => 32;
		public override string Name => "Crystal";
		public override string Icon => "textures/items/crystal.png";

		public override bool CanStackWith( InventoryItem other )
		{
			return true;
		}
	}
}
