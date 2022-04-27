using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrossbowShopItemTier1 : BaseWeaponShopItem<CrossbowItemTier1>
	{
		public override string Name => "Light Crossbow";
		public override string Description => "A light damage crossbow for dealing ranged damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 40
		};
		public override Type NextWeaponType => typeof( CrossbowItemTier1 );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_crossbow.png";
		}
	}
}
