using Facepunch.CoreWars.Inventory;
using System.Collections.Generic;
using Sandbox;
using System;

namespace Facepunch.CoreWars
{
	[Library]
	public class CrowbarShopItemTier3 : BaseWeaponShopItem<CrowbarItemTier3>
	{
		public override string Name => "Heavy Crowbar";
		public override string Description => "A heavy crowbar for dealing melee damage.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 70
		};
		public override Type PreviousWeaponType => typeof( CrowbarItemTier2 );
		public override Type NextWeaponType => typeof( CrowbarItemTier3 );
		public override Type RequiredUpgradeType => typeof( ArmoryUpgrade );

		public override string GetIcon( Player player )
		{
			return "textures/items/weapon_crowbar.png";
		}
	}
}
