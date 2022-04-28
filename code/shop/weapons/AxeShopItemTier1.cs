using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class AxeShopItemTier1 : BaseWeaponShopItem<AxeItemTier1>
	{
		public override string Name => "Light Axe";
		public override string Description => "A light damage axe for breaking wood.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_axe_1.png";
		}
	}
}
