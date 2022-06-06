using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class BlowtorchShopItem : BaseWeaponShopItem<BlowtorchItem>
	{
		public override string Name => "Blowtorch";
		public override string Description => "A simple tool for quickly melting plastic.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 30
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_blowtorch.png";
		}
	}
}
