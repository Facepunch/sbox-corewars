using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class AxeShopItemTier2 : BaseWeaponShopItem<AxeItemTier2>
	{
		public override string Name => "Medium Axe";
		public override string Description => "A medium axe for breaking wood.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 30
		};
		public override Type PreviousWeaponType => typeof( AxeItemTier1 );


		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_axe.png";
		}
	}
}
