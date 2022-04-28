using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class PickaxeShopItemTier2 : BaseWeaponShopItem<PickaxeItemTier2>
	{
		public override string Name => "Medium Pickaxe";
		public override string Description => "A medium pickaxe for breaking defensive blocks.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_pickaxe.png";
		}
	}
}
