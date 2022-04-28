using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class PickaxeShopItemTier1 : BaseWeaponShopItem<PickaxeItemTier1>
	{
		public override string Name => "Light Pickaxe";
		public override string Description => "A light damage pickaxe for breaking defensive blocks.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_pickaxe_1.png";
		}
	}
}
