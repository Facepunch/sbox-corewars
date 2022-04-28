using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrossbowShopItemTier2 : BaseWeaponShopItem<CrossbowItemTier2>
	{
		public override string Name => "Heavy Crossbow";
		public override string Description => "A heavy damage crossbow for dealing ranged damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( CrystalItem )] = 7
		};
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_crossbow.png";
		}
	}
}
