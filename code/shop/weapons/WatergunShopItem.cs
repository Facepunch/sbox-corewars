using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class WatergunShopItem : BaseWeaponShopItem<WatergunItem>
	{
		public override string Name => "Watergun";
		public override string Description => "A simple tool for neutralizing Vortex Bombs.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 20
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_watergun.png";
		}
	}
}
