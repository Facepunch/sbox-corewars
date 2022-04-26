using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class PickaxeShopItemTier3 : BaseWeaponShopItem<PickaxeItemTier3>
	{
		public override string Name => "Heavy Pickaxe";
		public override string Description => "A heavy pickaxe for breaking defensive blocks.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 60
		};
		public override Type PreviousWeaponType => typeof( PickaxeItemTier2 );


		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_pickaxe.png";
		}
	}
}
