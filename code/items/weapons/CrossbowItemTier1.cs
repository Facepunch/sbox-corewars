﻿using Facepunch.CoreWars.Inventory;
using Sandbox;
using System.IO;

namespace Facepunch.CoreWars
{
	[Library( "item_crossbow_1" )]
	public class CrossbowItemTier1 : WeaponItem
	{
		public override string WeaponName => "weapon_crossbow";
		public override string Icon => "textures/items/weapon_crossbow_1.png";
		public override string Name => "Light Crossbow";
		public override string Group => "crossbow";
		public override int Tier => 1;
	}
}
