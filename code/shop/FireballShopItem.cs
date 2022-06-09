﻿using System;
using System.Collections.Generic;
using Facepunch.CoreWars.Inventory;
using Sandbox;

namespace Facepunch.CoreWars
{
	[Library]
	public class FireballShopItem : BaseWeaponShopItem<FireballItem>
	{
		public override string Name => "Fireball";
		public override string Description => "A fireball which can melt plastic and damage other players.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 1,
			[typeof( IronItem )] = 4
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_fireball.png";
		}
	}
}
