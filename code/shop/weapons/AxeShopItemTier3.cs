using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class AxeShopItemTier3 : BaseWeaponShopItem<AxeItemTier3>
	{
		public override string Name => "Heavy Axe";
		public override string Description => "A heavy axe for breaking wood.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 60
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_axe.png";
		}
	}
}
